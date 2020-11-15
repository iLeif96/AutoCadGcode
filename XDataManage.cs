using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;


namespace AutoCadGcode
{
    public class XDataManage
    {

        public delegate void PropertiesChangeHandler(Entity entity, Properties properties);
        public static event PropertiesChangeHandler PropertiesChangeEvent;

        private const string KEY = "GCODE";
        public static void setXData(List<Entity> list, Properties props)
        {
            using (DocumentLock docLock = Global.doc.LockDocument())
            {
                using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
                {

                    RegAppTable acRegAppTbl;
                    acRegAppTbl = acTrans.GetObject(Global.dB.RegAppTableId, OpenMode.ForRead) as RegAppTable;

                    // Check to see if the Registered Applications table record for the custom app exists
                    if (acRegAppTbl.Has(props.KEY) == false)
                    {
                        using (RegAppTableRecord acRegAppTblRec = new RegAppTableRecord())
                        {
                            acRegAppTblRec.Name = props.KEY;

                            acTrans.GetObject(Global.dB.RegAppTableId, OpenMode.ForWrite);
                            acRegAppTbl.Add(acRegAppTblRec);
                            acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, true);
                        }
                    }

                    // Step through the objects in the selection set
                    foreach (Entity obj in list)
                    {
                        // Append the extended data to each object
                        Entity entity = acTrans.GetObject(obj.ObjectId, OpenMode.ForWrite) as Entity;
                        using (ResultBuffer buffer = props.ToBuffer())
                        {
                            entity.XData = buffer;

                            PropertiesChangeEvent?.Invoke(entity, props);
                        }
                    }

                    // Save the new object to the database
                    acTrans.Commit();
                }
            }
        }

        public static List<Properties> getXData(List<Entity> list)
        {
            using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
            {
                string msgstr = "";

                List<Properties> outLlist = new List<Properties>();
                Properties props = new Properties();

                // Step through the objects in the selection set
                foreach (Entity obj in list)
                {
                    Entity entity = acTrans.GetObject(obj.ObjectId, OpenMode.ForRead) as Entity;

                    // Get the extended data attached to each object for MY_APP
                    ResultBuffer rb = entity.GetXDataForApplication(props.KEY);

                    // Make sure the Xdata is not empty
                    if (rb != null)
                    {
                        props = Properties.FromBuffer(rb);
                        outLlist.Add(props);
                        // Get the values in the xdata and convert to Properties
                        foreach (TypedValue typeVal in rb)
                        {
                            msgstr = msgstr + "\n" + typeVal.TypeCode.ToString() + ":" + typeVal.Value;
                        }
                    }
                    else
                    {
                        msgstr = "NONE";
                    }

                    // Display the values returned
                    Global.editor.WriteMessage(XDataManage.KEY + " xdata on " + entity.GetType().ToString() + ":\n" + msgstr);

                    msgstr = "";
                }


                acTrans.Abort();

                return outLlist;
            }
        }
    }
}
