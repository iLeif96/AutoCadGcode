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
        public static GUI GuiInstance { get; set; }
        public static Validation ValidationInstance { get; set; }
        public static GcodeGenerator GcodeGeneratorInstance { get; set; }


        public static Dictionary<ObjectId, UserEntity> UEntitys { get; set; }

        public static void Greetings()
        {
            Global.Editor.WriteMessage("AutoCadGcode by Svetoslav Elkin\n");
        }

        public static void Start()
        {
            CreateObjects();
            CreateHandlings();
            CreateGlobalHandlings();
            API.ValidateEntities();
        }
        private static void CreateObjects()
        {
            UEntitys = new Dictionary<ObjectId, UserEntity>();
            GuiInstance = new GUI();
            ValidationInstance = new Validation();
            GcodeGeneratorInstance = new GcodeGenerator();
        }
        private static void CreateHandlings()
        {
            XDataManage.PropertiesChangeEvent += OnChangeProperties;
            API.EntitiesValidateEvent += OnValidateEntities;
            Global.DB.ObjectAppended += OnDatabaseChanged;
            //API.DocumentLoadedEvent += SetUserEntitys;
            //TODO
        }
        private static void CreateGlobalHandlings()
        {
            XDataManage.PropertiesChangeEvent += GuiInstance.OnChangeParametersHandler;
            Validation.ValidateEntitiesEvent += GuiInstance.OnValidationChangingHandler;
        }

        private static void OnDatabaseChanged(object sender, EventArgs e)
        {
            ValidationInstance.isValidated = false;
        }
        private static void OnValidateEntities()
        {
            try
            {
                UEntitys = new Dictionary<ObjectId, UserEntity>();
                SetUserEntitys();
                ValidationInstance.StartValidation(UEntitys.Values.ToList<UserEntity>());
            }
            catch(Exception e)
            {
                Global.Editor.WriteMessage(e.ToString());
            }
        }
        private static void OnChangeProperties(UserEntity uEntity)
        {
            ChangeColor(uEntity);
        }
        private static void ChangeColor(UserEntity uEntity)
        {
            if (uEntity != null)
            {
                using (DocumentLock docLock = Global.Doc.LockDocument())
                {
                    using (Transaction acTrans = Global.DB.TransactionManager.StartTransaction())
                    {
                        using (Entity temp = acTrans.GetObject(uEntity.ObjectId, OpenMode.ForWrite) as Entity)
                        {
                            if (uEntity.Properties != null)
                            {
                                if (uEntity.Properties.Printable == false)
                                    temp.Color = Color.FromRgb(150, 150, 150); //gray
                                else if (uEntity.Properties.Pumping == true)
                                    temp.Color = Color.FromRgb(50, 200, 50); //green
                                else
                                    temp.Color = Color.FromRgb(50, 50, 200); //blue
                            } else
                            {
                                temp.Color = Color.FromRgb(200, 50, 50); //red for invalid objects
                            }
                        }
                        acTrans.Commit();
                    }
                }
            }
        }
        private static void SetUserEntitys(List<Entity> list = null)
        {
            bool trash = false;
            if (list != null)
            {
                foreach (Entity entity in list)
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
                            Global.Editor.WriteMessage("Warrning. UEntity already stored\n");
                        }
                    }
                    else
                    {
                        trash = true;
                        ChangeColor(new UserEntity(entity, null));
                    }
                }
                if (trash == true)
                    Global.Editor.WriteMessage("Not all entites in document have available properties\n");
            }
            else
                SetUserEntitys(SelectAllObjectsFromAc()); 
        }
        private static List<Entity> SelectAllObjectsFromAc()
        {
            List<Entity> list = new List<Entity>();
            try
            {
                using (Transaction acTrans = Global.TM.StartTransaction())
                {
                    BlockTable bTable = acTrans.GetObject(Global.DB.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord bTableRecord = acTrans.GetObject(bTable[BlockTableRecord.ModelSpace],
                        OpenMode.ForRead) as BlockTableRecord;

                    foreach (ObjectId objectId in bTableRecord)
                    {
                        Entity entity = Global.TM.GetObject(objectId, OpenMode.ForRead) as Entity;

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
