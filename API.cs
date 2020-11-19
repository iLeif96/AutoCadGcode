using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    public class API
    {
        public delegate void SetPrintableHandler(UserEntity userEntity);
        public delegate void SetNonPrintableHandler(UserEntity userEntity);
        public delegate void PumpingChangeHandler(UserEntity userEntity);
        public delegate void FirstChangeHandler(UserEntity userEntity);
        public delegate void LastChangeHandler(UserEntity userEntity);
        public delegate void OrderChangeHandler(UserEntity userEntity);
        public delegate void StopAndPumpHandler(UserEntity userEntity);
        public delegate void EntitiesValidateHandler();
        public delegate void BuildGcodeHandler();

        public static event SetPrintableHandler SetPrintableEvent;
        public static event SetNonPrintableHandler SetNonPrintableEvent;
        public static event PumpingChangeHandler PumpingChangeEvent;
        public static event FirstChangeHandler FirstChangeEvent;
        public static event LastChangeHandler LastChangeEvent;
        public static event OrderChangeHandler OrderChangeEvent;
        public static event StopAndPumpHandler StopAndPumpEvent;
        public static event EntitiesValidateHandler EntitiesValidateEvent;
        public static event BuildGcodeHandler BuildGcodeEvent;



        /**
         * Debug commands
         */

        [CommandMethod("SAYHI")]
        public static void SayHi()
        {
            Global.Editor.WriteMessage("NOW IS OK");
        }

        /**
         * Properties changing
         */

        [CommandMethod("SETFIRST", CommandFlags.UsePickSet)]
        public static void SetFirst(Entity entity = null)
        {
            List<Entity> list = null;
            if (entity == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
                entity = list.Last<Entity>();
            }

            UserEntity uEntity = XDataManage.getXData(entity);

            if (uEntity == null)
                uEntity = new UserEntity(entity, new Properties());

            uEntity.Properties.Printable = false;
            uEntity.Properties.First = true;

            uEntity = XDataManage.setXData(uEntity);

            FirstChangeEvent?.Invoke(uEntity);
        }

        [CommandMethod("SETLAST", CommandFlags.UsePickSet)]
        public static void SetLast(Entity entity = null)
        {
            List<Entity> list = null;
            if (entity == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
                entity = list.Last<Entity>();
            }

            UserEntity uEntity = XDataManage.getXData(entity);

            if (uEntity == null)
                uEntity = new UserEntity(entity, new Properties());

            uEntity.Properties.Printable = false;
            uEntity.Properties.Last = true;

            uEntity = XDataManage.setXData(uEntity);

            LastChangeEvent?.Invoke(uEntity);
        }

        public static int prevOrder = -1;
        [CommandMethod("SETORDER", CommandFlags.UsePickSet)]
        public static void SetOrder(Properties properties = null)
        {
            int order = -1;
            if (properties != null)
                order = properties.Order;

            PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            if (list.Count == 0)
                return;

            if (order < 0)
            {
                PromptIntegerOptions pOpts = new PromptIntegerOptions(
                        "Пожалуйста, введите порядковый номер линии в чертеже: ");
                pOpts.LowerLimit = -1;
                pOpts.DefaultValue = API.prevOrder + 1;
                PromptIntegerResult promptInteger = Global.Doc.Editor.GetInteger(pOpts);
                order = promptInteger.Value;
                if (order < 0)
                {
                    Global.Editor.WriteMessage("Порядковый номер не может быть меньше нуля для печатной линии\n");
                    return;
                }
            }
            API.prevOrder = order;
            foreach (Entity entity in list)
            {
                UserEntity userEntity = XDataManage.getXData(entity);

                if (userEntity == null)
                    userEntity = new UserEntity(entity, new Properties());

                userEntity.Properties.Printable = true;
                userEntity.Properties.Order = order;

                userEntity = XDataManage.setXData(userEntity);
                OrderChangeEvent?.Invoke(userEntity);
            }
        }

        [CommandMethod("SETSTOPANDPUMP", CommandFlags.UsePickSet)]
        public static void SetStopAndPump(Properties properties = null)
        {
            int stopAndPump = -1;
            if (properties != null)
                stopAndPump = properties.StopAndPump;

            PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            if (list.Count > 1)
                return;

            if (stopAndPump == -1)
            {
                PromptIntegerOptions pOpts = new PromptIntegerOptions(
                        "Пожалуйста, введите время прокачки в милисекундах: ");
                pOpts.LowerLimit = 0;
                PromptIntegerResult promptInteger = Global.Doc.Editor.GetInteger(pOpts);
                stopAndPump = promptInteger.Value;
            }
            
            foreach (Entity entity in list)
            {
                UserEntity userEntity = XDataManage.getXData(entity);

                if (userEntity == null)
                    userEntity = new UserEntity(entity, new Properties());

                userEntity.Properties.Printable = false;
                userEntity.Properties.Command = true;
                userEntity.Properties.StopAndPump = stopAndPump;

                userEntity = XDataManage.setXData(userEntity);
                StopAndPumpEvent?.Invoke(userEntity);
            }
        }

        [CommandMethod("SETPRINTABLE", CommandFlags.UsePickSet)]
        public static void SetPrintable(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
            }
            UserEntity uEntity;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                uEntity = XDataManage.getXData(entity);
                if (uEntity == null)
                    uEntity = new UserEntity(entity, new Properties());

                uEntity.Properties.Printable = true;

                uEntity = XDataManage.setXData(uEntity);

                SetPrintableEvent?.Invoke(uEntity);
            }
        }

        [CommandMethod("SETNONPRINTABLE", CommandFlags.UsePickSet)]
        public static void SetNonPrintable(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
            }
            UserEntity uEntity;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                uEntity = XDataManage.getXData(entity);
                if (uEntity == null)
                    uEntity = new UserEntity(entity, new Properties());

                uEntity.Properties.Printable = false;
                uEntity.Properties.Pumping = false;

                uEntity = XDataManage.setXData(uEntity);

                SetNonPrintableEvent?.Invoke(uEntity);
            }
        }

        [CommandMethod("SETPUMPINGTRUE", CommandFlags.UsePickSet)]
        public static void SetPumpingTrue(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
            }
            UserEntity uEntity;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                uEntity = XDataManage.getXData(entity);
                if (uEntity == null)
                    uEntity = new UserEntity(entity, new Properties());

                uEntity.Properties.Printable = true;
                uEntity.Properties.Pumping = true;

                uEntity = XDataManage.setXData(uEntity);

                PumpingChangeEvent?.Invoke(uEntity);
            }
        }

        [CommandMethod("SETPUMPINGFALSE", CommandFlags.UsePickSet)]
        public static void SetPumpingFalse(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
            }
            UserEntity uEntity;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                uEntity = XDataManage.getXData(entity);
                if (uEntity == null)
                    uEntity = new UserEntity(entity, new Properties());

                uEntity.Properties.Printable = true;
                uEntity.Properties.Pumping = false;

                uEntity = XDataManage.setXData(uEntity);

                PumpingChangeEvent?.Invoke(uEntity);
            }
        }

        [CommandMethod("SETXDATA", CommandFlags.UsePickSet)]
        public static void SetXData(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                if (list.Count == 0)
                    return;
            }

            foreach (Entity entity in list)
            {
                XDataManage.setXData(new UserEntity(entity, new Properties()));
            }
        }

        [CommandMethod("GETXDATA", CommandFlags.UsePickSet)]
        public static List<UserEntity> GetXData(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.Doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
            }

            return XDataManage.getXData(list);
        }

        [CommandMethod("VALIDATEENTITIES")]
        public static void ValidateEntities()
        {
            EntitiesValidateEvent?.Invoke();
        }

        [CommandMethod("BUILDGCODE")]
        public static void BuildGcode()
        {
            BuildGcodeEvent?.Invoke();
        }
        
        /**
         * Addition functions
         */
        public static List<Entity> ListFromSelecion(PromptSelectionResult acSSPrompt)
        {
            List<Entity> list = new List<Entity>();

            // If the prompt status is OK, objects were selected
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                SelectionSet acSSet = acSSPrompt.Value;

                using (Transaction acTrans = Global.DB.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        // Open the selected object for read
                        list.Add(acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as Entity);
                    }

                    acTrans.Abort();
                }
            }
            else
                Global.Editor.WriteMessage("There is problem in selection");

            return list;
        }

        public static UserEntity FindUserEntityByObjectId(ObjectId objectId)
        {
            UserEntity uEntity = null;
            using (Transaction acTrans = Global.DB.TransactionManager.StartTransaction())
            {
                var ent = acTrans.GetObject(objectId, OpenMode.ForRead) as Entity;
                uEntity = XDataManage.getXData(ent);
                acTrans.Abort();
            }
            return uEntity;
        }
    }
}
