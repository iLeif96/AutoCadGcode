using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace AutoCadGcode
{
    public class Process
    {

        public Process()
        {
            API.PumpingChangeEvent += ChangeColor;
            API.DocumentLoadedEvent += SetUserEntitys;
        }

        public static void ChangeColor(UserEntity uEntity)
        {
            if (uEntity != null)
            {
                using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
                {
                    using (Entity temp = acTrans.GetObject(uEntity.ObjectId, OpenMode.ForWrite) as Entity)
                    {
                        if (uEntity.properties.Pumping == true)
                            temp.Color = Color.FromRgb(50, 200, 50); //green
                        else
                            temp.Color = Color.FromRgb(50, 50, 200); //blue
                    }
                    acTrans.Commit(); 
                }
            }
        }

        public static void SetUserEntitys(List<Entity> list = null)
        {
            if (list != null)
            {
                foreach (Entity entity in list)
                {
                    SetUserEntitys(entity);
                }
            }
            else
            {
                SetUserEntitys(SelectAllObjectsFromAc());
            }
        }

        public static void SetUserEntitys(Entity entity)
        {
            if (list == null)
                return;

            Global.uEntitys.Add();
        }



        public static List<Entity> SelectAllObjectsFromAc()
        {
            List<Entity> list = null;
            using (Transaction acTrans = Global.dB.TransactionManager.StartTransaction())
            {
                BlockTable bTable = acTrans.GetObject(Global.dB.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord bTableRecord = acTrans.GetObject(bTable[BlockTableRecord.ModelSpace],
                    OpenMode.ForRead) as BlockTableRecord;
                
                foreach (ObjectId objectId in bTableRecord)
                {
                    Entity entity = acTrans.GetObject(objectId, OpenMode.ForRead) as Entity;
                    
                    list.Add(entity);
                }
            }
            return list;
        }
    }
}
