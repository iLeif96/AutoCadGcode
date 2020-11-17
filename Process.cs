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
    public static class Process
    {
        public static GUI Gui { get; set; }

        public static Validation Validation { get; set; }

        public static Dictionary<ObjectId, UserEntity> UEntitys { get; set; }

        public static void Start()
        {
            CreateObjects();
            CreateHandling();
            API.ValidateEntities();

            Global.editor.WriteMessage("AutoCadGcode by Svetoslav Elkin now is ready to work/n");
        }
        private static void CreateObjects()
        {
            UEntitys = new Dictionary<ObjectId, UserEntity>();
            Gui = new GUI();
            Validation = new Validation();
        }
        private static void CreateHandling()
        {
            XDataManage.PropertiesChangeEvent += OnChangeProperties;
            API.EntitiesValidateEvent += OnValidateEntities;
            Global.dB.ObjectAppended += OnDatabaseChanged;
            //API.DocumentLoadedEvent += SetUserEntitys;
            //TODO
        }
        private static void OnDatabaseChanged(object sender, EventArgs e)
        {
            Validation.isValidated = false;
        }
        private static void OnValidateEntities()
        {
            try
            {
                UEntitys = new Dictionary<ObjectId, UserEntity>();
                SetUserEntitys();
                Validation.StartValidation(UEntitys.Values.ToList<UserEntity>());
            }
            catch(Exception e)
            {
                Global.editor.WriteMessage(e.ToString());
            }
        }
        private static void OnChangeProperties(UserEntity uEntity)
        {
            OnChangeColor(uEntity);
        }
        private static void OnChangeColor(UserEntity uEntity)
        {
            if (uEntity != null)
            {
                using (DocumentLock docLock = Global.doc.LockDocument())
                {
                    using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
                    {
                        using (Entity temp = acTrans.GetObject(uEntity.ObjectId, OpenMode.ForWrite) as Entity)
                        {
                            if (uEntity.properties.Printable == false)
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
        private static void SetUserEntitys(List<Entity> list = null)
        {
            if (list != null)
                foreach (Entity entity in list)
                    SetUserEntitys(entity);
            else
                SetUserEntitys(SelectAllObjectsFromAc()); 
        }
        private static void SetUserEntitys(Entity entity)
        {
            if (entity == null)
                return;

            UserEntity uEntity = XDataManage.getXData(entity);

            if (uEntity != null)
            {
                if (!UEntitys.ContainsKey(entity.ObjectId))
                    UEntitys.Add(uEntity.ObjectId, uEntity);
                else
                {
                    UEntitys[uEntity.ObjectId] = uEntity;
                    Global.editor.WriteMessage("Warrning. Object already stored");
                }
            }
            else
                Global.editor.WriteMessage("Not all entites in document have available properties");
        }
        private static List<Entity> SelectAllObjectsFromAc()
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
