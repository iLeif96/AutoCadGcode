using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;

namespace AutoCadGcode
{
    public class Process
    {

        public Process()
        {
            XDataManage.PropertiesChangeEvent += OnChangeProperties;
            API.EntityesValidateEvent += OnValidateEntityes;
            //API.DocumentLoadedEvent += SetUserEntitys;
            //TODO
            //Global.dB.ObjectAppended += OnDatabaseChanged;
            
        }

        public void OnDatabaseChanged(object sender, EventArgs e)
        {
            SetUserEntitys();
        }

        public static void OnValidateEntityes()
        {
            Global.uEntitys = new Dictionary<ObjectId, UserEntity>();
            SetUserEntitys();
        }

        public static void OnChangeProperties(UserEntity uEntity)
        {
            OnChangeColor(uEntity);
        }
        public static void OnChangeColor(UserEntity uEntity)
        {
            if (uEntity != null)
            {
                using (DocumentLock docLock = Global.doc.LockDocument())
                {
                    using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
                    {
                        using (Entity temp = acTrans.GetObject(uEntity.ObjectId, OpenMode.ForWrite) as Entity)
                        {
                            if (uEntity.properties.First == true || uEntity.properties.Last == true)
                                temp.Color = Color.FromRgb(150, 150, 150); //gray
                            else if (uEntity.properties.Pumping == true)
                                temp.Color = Color.FromRgb(50, 200, 50); //green
                            else
                                temp.Color = Color.FromRgb(50, 50, 200); //blue
                        }
                        acTrans.Commit();
                    }
                }
            }
        }
        public static void SetUserEntitys(List<Entity> list = null)
        {
            if (list != null)
                foreach (Entity entity in list)
                    SetUserEntitys(entity);
            else
                SetUserEntitys(SelectAllObjectsFromAc());
        }
        public static void SetUserEntitys(Entity entity)
        {
            if (entity == null)
                return;

            Properties props = XDataManage.getXData(entity);
            UserEntity userEntity;

            if (props == null)
                userEntity = XDataManage.setXData(new UserEntity(entity, new Properties()));
            else
                userEntity = new UserEntity(entity, props);

            if (!Global.uEntitys.ContainsKey(entity.ObjectId))
                Global.uEntitys.Add(userEntity.ObjectId, userEntity);
            else
            {
                Global.uEntitys[userEntity.ObjectId] = userEntity;
                Global.editor.WriteMessage("Warrning. Object already stored");
            }
        }



        public static List<Entity> SelectAllObjectsFromAc()
        {
            List<Entity> list = new List<Entity>();
            try
            {
                using (Transaction acTrans = Global.tM.StartTransaction())
                {
                    BlockTable bTable = acTrans.GetObject(Global.dB.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord bTableRecord = acTrans.GetObject(bTable[BlockTableRecord.ModelSpace],
                        OpenMode.ForRead) as BlockTableRecord;


                    foreach (ObjectId objectId in bTableRecord)
                    {
                        Entity entity = Global.tM.GetObject(objectId, OpenMode.ForRead) as Entity;

                        list.Add(entity);
                    }
                    acTrans.Dispose();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return list;
        }
    }
}
