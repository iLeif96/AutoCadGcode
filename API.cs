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
    public class API : IExtensionApplication
    {
        public delegate void PumpingChangeHandler(UserEntity userEntity);
        public delegate void FirstChangeHandler(UserEntity userEntity);
        public delegate void LastChangeHandler(UserEntity userEntity);
        public delegate void OrderChangeHandler(UserEntity userEntity);
        public delegate void DocumentLoadedHandler(List<Entity> list = null);
        public delegate void EntityesValidateHandler();
        public delegate void BuildGcodeHandler();

        public static event PumpingChangeHandler PumpingChangeEvent;
        public static event FirstChangeHandler FirstChangeEvent;
        public static event LastChangeHandler LastChangeEvent;
        public static event OrderChangeHandler OrderChangeEvent;
        public static event DocumentLoadedHandler DocumentLoadedEvent;
        public static event EntityesValidateHandler EntityesValidateEvent;
        public static event BuildGcodeHandler BuildGcodeEvent;



        /**
         * Debug commands
         */

        [CommandMethod("SAYHI")]
        public static void SayHi()
        {
            Global.editor.WriteMessage("NOW IS OK");
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
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                entity = list.Last<Entity>();
            }

            UserEntity uEntity = XDataManage.getXData(entity);

            if (uEntity == null)
                uEntity = new UserEntity(entity, new Properties());

            uEntity.properties.Printable = false;
            uEntity.properties.Last = true;

            uEntity = XDataManage.setXData(uEntity);

            LastChangeEvent?.Invoke(uEntity);
        }

        [CommandMethod("SETLAST", CommandFlags.UsePickSet)]
        public static void SetLast(Entity entity = null)
        {
            List<Entity> list = null;
            if (entity == null)
            {
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
                entity = list.Last<Entity>();
            }

            UserEntity uEntity = XDataManage.getXData(entity);

            if (uEntity == null)
                uEntity = new UserEntity(entity, new Properties());

            uEntity.properties.Printable = false;
            uEntity.properties.Last = true;

            uEntity = XDataManage.setXData(uEntity);

            LastChangeEvent?.Invoke(uEntity);
        }

        public static int prevOrder = -1;
        [CommandMethod("SETORDER", CommandFlags.UsePickSet)]
        public static void SetOrder(Properties propertiesOrder = null)
        {
            int order = -1;
            if (propertiesOrder != null)
                order = propertiesOrder.Order;

            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            if (order < 0)
            {
                PromptIntegerOptions pOpts = new PromptIntegerOptions(
                        "Пожалуйста, введите порядковый номер линии в чертеже: ");
                pOpts.LowerLimit = -1;
                pOpts.DefaultValue = API.prevOrder + 1;
                PromptIntegerResult promptInteger = Global.doc.Editor.GetInteger(pOpts);
                order = promptInteger.Value;
                if (order < 0)
                {
                    Global.editor.WriteMessage("Порядковый номер не может быть меньше нуля\n");
                    return;
                }
            }
            API.prevOrder = order;
            foreach (Entity entity in list)
            {
                UserEntity userEntity = XDataManage.getXData(entity);

                if (userEntity == null)
                    userEntity = new UserEntity(entity, new Properties());

                userEntity.properties.Printable = true;
                userEntity.properties.Order = order;

                userEntity = XDataManage.setXData(userEntity);
                OrderChangeEvent?.Invoke(userEntity);
            }
        }

        [CommandMethod("SETPUMPINGTRUE", CommandFlags.UsePickSet)]
        public static void SetPumpingTrue(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
            }
            UserEntity uEntity;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                uEntity = XDataManage.getXData(entity);
                if (uEntity == null)
                    uEntity = new UserEntity(entity, new Properties());

                uEntity.properties.Printable = true;
                uEntity.properties.Pumping = true;

                uEntity = XDataManage.setXData(uEntity);

                PumpingChangeEvent?.Invoke(uEntity);
            }
        }

        [CommandMethod("SETPUMPINGFALSE", CommandFlags.UsePickSet)]
        public static void SetPumpingFalse(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
            }
            UserEntity uEntity;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                uEntity = XDataManage.getXData(entity);
                if (uEntity == null)
                    uEntity = new UserEntity(entity, new Properties());

                uEntity.properties.Printable = true;
                uEntity.properties.Pumping = false;

                uEntity = XDataManage.setXData(uEntity);

                PumpingChangeEvent?.Invoke(uEntity);
            }
        }

        [CommandMethod("SETXDATA", CommandFlags.UsePickSet)]
        public static void SetXData(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
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
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
            }

            return XDataManage.getXData(list);
        }

        [CommandMethod("VALIDATEENTITYES")]
        public static void ValidateEntityes()
        {
            EntityesValidateEvent?.Invoke();
        }

        [CommandMethod("BUILDGCODE")]
        public static void BuildGcode()
        {
            BuildGcodeEvent?.Invoke();
        }
        


        public void Initialize()
        {
            Global.editor.WriteMessage("HELLOW/n");

            ValidateEntityes();
            DocumentLoadedEvent?.Invoke();
        }

        public void Terminate()
        {

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

                using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
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
                new System.Exception("There is problem in selection");

            return list;
        }

        public static UserEntity FindUserEntityByObjectId(ObjectId objectId)
        {
            UserEntity uEntity = null;
            using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
            {
                var ent = acTrans.GetObject(objectId, OpenMode.ForRead) as Entity;
                uEntity = XDataManage.getXData(ent);
                acTrans.Abort();
            }
            return uEntity;
        }
    }
}
