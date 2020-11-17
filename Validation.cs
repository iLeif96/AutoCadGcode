using Autodesk.AutoCAD.DatabaseServices;
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

        public List<UserEntity> forPrint = new List<UserEntity>();
        public List<UserEntity> notForPrint = new List<UserEntity>();

        public void StartValidation(List<UserEntity> uEntitys)
        {
            isValidated = false;

            forPrint.Clear();
            notForPrint.Clear();

            if (uEntitys == null || uEntitys.Count == 0)
            {
                throw new Exception("User Entities collection is empty");
            }

            UserEntity firstEntity = null;
            UserEntity lastEntity = null;

            Type type = null;

            int orderMax, orderMin, currOrder;
            orderMax = orderMin = uEntitys[0].properties.Order;
            currOrder = -1;

            //Prevent validation iteration
            foreach (UserEntity uEntity in uEntitys)
            {
                /**
                 * Checking available types for entities 
                 */
                if (uEntity.CheckType() == false)
                    throw new Exception("For generating Gcode must using only Line, Polyline and Arc entities");

                /**
                * Finding Firt and Last lines
                */
                if (uEntity.properties.First == true)
                {
                    if (firstEntity != null)
                        throw new Exception("First Line have to define only one time");
                    firstEntity = uEntity;
                }

                if (uEntity.properties.Last == true)
                {
                    if (lastEntity != null)
                        throw new Exception("Last Line have to define only one time");
                    lastEntity = uEntity;
                }

                /**
                 * Filling lists
                 */
                if (uEntity.properties.Printable == true)
                {
                    if (currOrder == uEntity.properties.Order)
                        throw new Exception("The order of entities cannot have multiple equal values");

                    currOrder = uEntity.properties.Order;
                    orderMax = Math.Max(orderMax, currOrder);
                    orderMin = Math.Min(orderMin, currOrder);
                    forPrint.Add(uEntity);
                }
                else
                    notForPrint.Add(uEntity);
            }

            /**
             * Checking values after prevent validation
             */
            if (forPrint.Count == 0)
                throw new Exception("There are no printable entities");

            if (orderMin == -1 || orderMax <= orderMin)
                throw new Exception("Problem in entities ordering");

            /**
             * Sorting printable entities by order
             */
            forPrint = TreeNode.TreeFromList(forPrint).ToSortList();

            foreach (UserEntity uEntity in forPrint)
            {
                Entity entity = uEntity.entity;
            }

            isValidated = true;
        }
    }
}
