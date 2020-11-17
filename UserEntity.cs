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

            if (type.Name != "Line" && type.Name != "Polyline" && type.Name != "Arc")
                result = true;

            if (type.Name == "Polyline")
            {
                try
                {
                    Polyline polyline = entity as Polyline;
                    //foreach (Entity entity in polyline.)
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
