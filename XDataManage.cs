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

        public delegate void PropertiesChangeHandler(UserEntity userEntity);
        public static event PropertiesChangeHandler PropertiesChangeEvent;

        private const string KEY = "GCODE";
        public static List<UserEntity> setXData(List<UserEntity> uEntities)
        {
            // Step through the objects in the set
            foreach (UserEntity uEntity in uEntities)
                setXData(uEntity);
            
            return uEntities;
        }

        public static UserEntity setXData(UserEntity uEntity)
        {
            using (DocumentLock docLock = Global.doc.LockDocument())
            {
                using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
                {

                    RegAppTable acRegAppTbl;
                    acRegAppTbl = acTrans.GetObject(Global.dB.RegAppTableId, OpenMode.ForRead) as RegAppTable;

                    // Check to see if the Registered Applications table record for the custom app exists
                    if (acRegAppTbl.Has(uEntity.Properties.KEY) == false)
                    {
                        using (RegAppTableRecord acRegAppTblRec = new RegAppTableRecord())
                        {
                            acRegAppTblRec.Name = uEntity.Properties.KEY;
                            acTrans.GetObject(Global.dB.RegAppTableId, OpenMode.ForWrite);
                            acRegAppTbl.Add(acRegAppTblRec);
                            acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, true);
                        }
                    }

                    // Append the extended data to each object
                    Entity entity = acTrans.GetObject(uEntity.ObjectId, OpenMode.ForWrite) as Entity;
                    using (ResultBuffer buffer = uEntity.Properties.ToBuffer())
                    {
                        entity.XData = buffer;
                        PropertiesChangeEvent?.Invoke(uEntity);
                    }
                    // Save the new object to the database
                    acTrans.Commit();
                    Global.editor.WriteMessage(uEntity.Properties.ToString());
                    return uEntity;
                }
            }
        }

        public static List<UserEntity> getXData(List<Entity> list)
        {
            List<UserEntity> outLlist = new List<UserEntity>();
            UserEntity uEentity;

            // Step through the objects in the selection set
            foreach (Entity obj in list)
            {
                uEentity = getXData(obj);
                if (uEentity != null)
                    outLlist.Add(uEentity);
            }

            return outLlist;    
        }

        public static UserEntity getXData(Entity entity)
        {
            using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
            {
                Properties props = new Properties();

                Entity _entity = acTrans.GetObject(entity.ObjectId, OpenMode.ForRead) as Entity;

                // Get the extended data attached to each object for MY_APP
                ResultBuffer rb = _entity.GetXDataForApplication(props.KEY);

                // Make sure the Xdata is not empty
                if (rb == null)
                    return null;

                props = Properties.FromBuffer(rb);
                acTrans.Abort();
                return new UserEntity(_entity, props);
            }
        }
    }
}
