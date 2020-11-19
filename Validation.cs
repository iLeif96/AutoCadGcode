using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    public class Validation
    {
        public delegate void ValidationChangedHandler(bool isValidatied);
        public static event ValidationChangedHandler ValidateEntitiesEvent;

        private bool _isValidated = false;
        public bool isValidated
        {
            get
            {
                return _isValidated;
            }
            set
            {
                _isValidated = value;
                ValidateEntitiesEvent?.Invoke(value);
            }
        }

        public List<ValidatedObject> validList = new List<ValidatedObject>();
        private List<UserEntity> forPrint = new List<UserEntity>();
        private List<UserEntity> commands = new List<UserEntity>();

        public void StartValidation(List<UserEntity> uEntitys)
        {
            isValidated = false;

            if (uEntitys == null || uEntitys.Count == 0)
                throw new Exception("User Entities collection is empty\n");

            //Clear all supported lists
            validList.Clear();
            forPrint.Clear();
            commands.Clear();

            //Set temporary objects
            Entity temp = null;
            UserEntity firstEntity = null;
            UserEntity lastEntity = null;

            List<IntermediateObject> tempInterList = new List<IntermediateObject>();

            int orderMax, orderMin, currOrder, index;
            orderMax = orderMin = uEntitys[0].Properties.Order;
            currOrder = -1;

            //Prevent validation iteration
            foreach (UserEntity uEntity in uEntitys)
            {
                /**
                 * Checking available types for entities 
                 */
                if (UserEntity.CheckType(uEntity.Type) == false)
                    throw new Exception("For generating Gcode must using only Line, Polyline and Arc entities\n");

                /**
                 * Finding Firt and Last lines. It`s need for orderinig inside entities
                 */
                if (uEntity.Properties.First == true)
                {
                    if (firstEntity != null)
                        throw new Exception("First Line have to define only one time\n");
                    firstEntity = uEntity;
                }
                else if (uEntity.Properties.Last == true)
                {
                    if (lastEntity != null)
                        throw new Exception("Last Line have to define only one time\n");
                    lastEntity = uEntity;
                }

                //Actions for separate printable and command entities
                if (uEntity.Properties.Printable == true)
                {
                    /**
                    * Convert all printable entities to polylines
                    */
                    if (!uEntity.ConvertToPolyline())
                        throw new Exception("Problem with converting entities to polyline\n");
                    /**
                     * Filling lists
                     */
                    if (currOrder == uEntity.Properties.Order)
                        throw new Exception("The order of entities cannot have multiple equal values\n");

                    currOrder = uEntity.Properties.Order;
                    orderMax = Math.Max(orderMax, currOrder);
                    orderMin = Math.Min(orderMin, currOrder);
                    forPrint.Add(uEntity);
                }
                else
                {
                    if (uEntity.Type != AvailableTypes.LINE)
                        throw new Exception("For command entities available use only simple Lines\n");
                    if (uEntity.Properties.Command)
                        commands.Add(uEntity);
                }
            }

            /**
             * Checking values after prevent validation
             */
            if (forPrint.Count == 0)
                throw new Exception("There are no printable entities\n");
            else if (forPrint.Count == 1)
            {
                if (orderMax != orderMin || orderMin != currOrder)
                    throw new Exception("This is devil tricks. Ordering problem\n");
            }
            else if (forPrint.Count > 1)
            {
                if (orderMin == -1 || (orderMax <= orderMin))
                    throw new Exception("Problem in entities ordering\n");
            }

            /**
             * Sorting printable entities by order 
             */
            forPrint = TreeNode.TreeFromList(forPrint).ToSortList();

            /**
             * Mark userEntities according with continiously path;
             * Mark "isNeedReverse" if entity have to reverse.
             */

            // Marking first line
            index = forPrint.First().IndexOfTouching(firstEntity.StartPoint);
            if (index == -1)
                index = forPrint.First().IndexOfTouching(firstEntity.EndPoint);
            if (index == -1)
                throw new Exception("First line not connected to first entity\n");
            else if (index > 0)
                forPrint.First().MarkToReverse();

            //Marking remaining 
            for (int i = 0; i < forPrint.Count - 1; i++)
            {
                index = forPrint[i + 1].IndexOfTouching(forPrint[i].StartPoint);
                if (index == -1)
                    index = forPrint[i + 1].IndexOfTouching(forPrint[i].EndPoint);
                if (index == -1)
                    throw new Exception("Entity with Order [" +
                        currOrder + "] has not connected to next segment\n");
                else if (index > 0)
                    forPrint[i + 1].MarkToReverse();
            }
            //Check last line
            index = forPrint.Last().IndexOfTouching(lastEntity.StartPoint);
            if (index == -1)
                index = forPrint.Last().IndexOfTouching(lastEntity.EndPoint);
            if (index == -1)
                throw new Exception("Last line not connected to last entity\n");

            /**
             * Step that makes Validated objects list (function ToIntermediateList makes Reverse by marks)
             */
            foreach (UserEntity uEntity in forPrint)
            {
                tempInterList = uEntity.ToIntermediateList();
                if (tempInterList.Count > 0)
                    foreach (IntermediateObject iObj in tempInterList)
                        validList.Add(new ValidatedObject(iObj, new ValidProperties(uEntity.Properties)));
                else
                    throw new Exception("UserEntity with order [" + uEntity.Properties.Order +
                        "] returned empty inermediate state");
            }

            /**
             * Final continiously path check
             */
            // Check first segment
            
            if (validList.First().IndexOfTouching(firstEntity.StartPoint) == -1 &&
                validList.First().IndexOfTouching(firstEntity.EndPoint) == -1)
                throw new Exception("First line not connected to first segment\n");

            //Checking remaining 
            for (int i = 0; i < forPrint.Count - 1; i++)
            {
                if (validList[i + 1].IndexOfTouching(validList[i].EndPoint) > 0)
                    throw new Exception("Segment [" + i + "] with Order [" +
                        forPrint[i].Properties.Order + "] has not connected to next segment\n");
            }
            //Check last segment
            if (validList.Last().IndexOfTouching(lastEntity.StartPoint) == -1 &&
                validList.Last().IndexOfTouching(lastEntity.EndPoint) == -1)
                throw new Exception("Last line not connected to last segment\n");

            /**
             * Checking command lines
             */

            foreach (UserEntity uEntity in commands)
            {
                /**
                * Checking notPrintable lines touching to printable entities
                */
                //Use for check that only one point of line touched to printable segments
                index = -1;

                for (int i = 0; i < validList.Count; i++)
                {
                    if (validList[i].IsEmpty == false)
                    {
                        if (validList[i].IndexOfTouching(uEntity.StartPoint) == 0)
                            index = i;

                        if (validList[i].IndexOfTouching(uEntity.EndPoint) == 0)
                        {
                            if (index != -1)
                                throw new Exception("Command line has to touch printing segments only once\n");

                            index = i;
                        }
                    }
                }

                if (index != -1)
                    validList.Insert(index, new ValidatedObject(new ValidProperties(uEntity.Properties)));
            }
            
            isValidated = true;
        }
    }
}
