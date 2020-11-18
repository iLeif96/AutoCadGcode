using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    public class UserEntity
    {
        public Entity entity;
        public Properties properties;
        public ObjectId ObjectId;

        public UserEntity(Entity entity, Properties properties)
        {
            this.entity = entity;
            this.properties = properties;
            this.ObjectId = entity.ObjectId;
        }
        public bool CheckType(Entity entity = null)
        {
            bool result = false;
            if (entity == null)
                entity = this.entity;

            Type type = entity.GetType();

            if (type ==  AvailableTypes.LINE || type == AvailableTypes.POLYLINE || type == AvailableTypes.ARC)
                result = true;

            if (type == AvailableTypes.POLYLINE)
            {
                try
                {
                    Polyline polyline = (Polyline)entity as Polyline;
                }
                catch (Exception e)
                {
                    return result = false;
                }
            }

            return result;
        }
    }
}
