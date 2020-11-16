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
        public delegate void DocumentLoadedHandler(List<Entity> list = null);
        public delegate void EntityesValidateHandler();

        public static event PumpingChangeHandler PumpingChangeEvent;
        public static event FirstChangeHandler FirstChangeEvent;
        public static event LastChangeHandler LastChangeEvent;
        public static event DocumentLoadedHandler DocumentLoadedEvent;
        public static event EntityesValidateHandler EntityesValidateEvent;



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
        public void SetFirst()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            Properties props;

            if (list.Count != 1)
                return;

            List<Properties> resList = XDataManage.getXData(list);

            if (resList.Count != 1)
                props = new Properties();
            else
                props = resList[0];

            props.Last = false;
            props.First = true;

            UserEntity userEntity = XDataManage.setXData(new UserEntity(list[0], props));

            FirstChangeEvent?.Invoke(userEntity);
        }

        [CommandMethod("SETLAST", CommandFlags.UsePickSet)]
        public static void SetLast()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            Properties props;

            if (list.Count != 1)
                return;

            List<Properties> resList = XDataManage.getXData(list);

            if (resList.Count != 1)
                props = new Properties();
            else
                props = resList[0];

            props.First = false;
            props.Last = true;

            UserEntity userEntity = XDataManage.setXData(new UserEntity(list[0], props));

            LastChangeEvent?.Invoke(userEntity);
        }

        [CommandMethod("SETPUMPINGTRUE", CommandFlags.UsePickSet)]
        public static void SetPumpingTrue()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            Properties props;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                props = XDataManage.getXData(entity);
                if (props == null)
                    props = new Properties();

                props.Pumping = true;

                UserEntity userEntity = XDataManage.setXData(new UserEntity(entity, props));

                PumpingChangeEvent?.Invoke(userEntity);
            }
        }

        [CommandMethod("SETPUMPINGFALSE", CommandFlags.UsePickSet)]
        public static void SetPumpingFalse()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            Properties props;

            if (list.Count < 1)
                return;

            foreach (Entity entity in list)
            {
                props = XDataManage.getXData(entity);
                if (props == null)
                    props = new Properties();

                props.Pumping = false;

                UserEntity userEntity = XDataManage.setXData(new UserEntity(entity, props));

                PumpingChangeEvent?.Invoke(userEntity);
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
        public static void GetXData(List<Entity> list = null)
        {
            if (list == null)
            {
                PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
                list = ListFromSelecion(acSSPrompt);
            }

            XDataManage.getXData(list);
        }

        [CommandMethod("VALIDATEENTITYES")]
        public static void ValidateEntityes()
        {
            EntityesValidateEvent?.Invoke();
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
        private static List<Entity> ListFromSelecion(PromptSelectionResult acSSPrompt)
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
    }
}
