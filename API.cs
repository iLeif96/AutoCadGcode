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


        public static event PumpingChangeHandler PumpingChangeEvent;
        public static event FirstChangeHandler FirstChangeEvent;
        public static event LastChangeHandler LastChangeEvent;
        public static event DocumentLoadedHandler DocumentLoadedEvent;


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

            XDataManage.setXData(list, props);

            if (Global.uEntitys.ContainsKey(list[0].ObjectId))
                FirstChangeEvent?.Invoke(Global.uEntitys[list[0].ObjectId]);
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

            XDataManage.setXData(list, props);

            if (Global.uEntitys.ContainsKey(list[0].ObjectId))
                LastChangeEvent?.Invoke(Global.uEntitys[list[0].ObjectId]);
        }

        [CommandMethod("SETPUMPINGTRUE", CommandFlags.UsePickSet)]
        public static void SetPumpingTrue()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            Properties props;

            if (list.Count < 1)
                return;

            List<Properties> resList;
            List<Entity> entList;
            props = new Properties();

            foreach (Entity entity in list)
            {
                entList = new List<Entity> { entity };
                resList = XDataManage.getXData(entList);
                if (resList.Count != 1)
                    props = new Properties();
                else
                    props = resList[0];

                props.Pumping = true;

                XDataManage.setXData(new List<Entity> { entity }, props);

                if (Global.uEntitys.ContainsKey(entity.ObjectId))
                    PumpingChangeEvent?.Invoke(Global.uEntitys[entity.ObjectId]);
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

            List<Properties> resList;
            List<Entity> entList;
            props = new Properties();

            foreach (Entity entity in list)
            {
                entList = new List<Entity> { entity };
                resList = XDataManage.getXData(entList);
                if (resList.Count != 1)
                    props = new Properties();
                else
                    props = resList[0];

                props.Pumping = false;


                XDataManage.setXData(new List<Entity> { entity }, props);

                if (Global.uEntitys.ContainsKey(entity.ObjectId))
                    PumpingChangeEvent?.Invoke(Global.uEntitys[entity.ObjectId]);
            }
        }

        [CommandMethod("SETXDATA", CommandFlags.UsePickSet)]
        public static void SetXData()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            XDataManage.setXData(list, new Properties());
        }

        [CommandMethod("GETXDATA", CommandFlags.UsePickSet)]
        public static void GetXData()
        {
            PromptSelectionResult acSSPrompt = Global.doc.Editor.GetSelection();
            List<Entity> list = ListFromSelecion(acSSPrompt);
            XDataManage.getXData(list);
        }


        public void Initialize()
        {
            Global.editor.WriteMessage("HELOW");
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
