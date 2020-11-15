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
    }


}
