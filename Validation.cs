using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    
    public class Validation
    {
        public delegate void ValidationChangedHandler(bool isValidatied);
        public static event ValidationChangedHandler ValidateEntityesEvent;

        private bool _isValidated = false;
        public bool isValidated
        {
            get
            {
                return _isValidated;
            }
            set
            {
                _isValidated = value;
                ValidateEntityesEvent?.Invoke(value);
            }
        }

        public List<UserEntity> forPrint = new List<UserEntity>();
        public List<UserEntity> notForPrint = new List<UserEntity>();

        public void StartValidation(List<UserEntity> uEntitys)
        {
            isValidated = false;

            if (uEntitys == null || Global.uEntitys.Count == 0)
            {
                throw new Exception("User Entityes collection is empty");
            }

            UserEntity firstEntity = null;
            UserEntity lastEntity = null;


            foreach (UserEntity uEntity in uEntitys)
            {
                /**
                 * Finding Firt and Last lines
                 */
                if (uEntity.properties.Last == true)
                {
                    if (firstEntity != null)
                        throw new Exception("First Line have to define only one time");
                    firstEntity = uEntity;
                }

                if (uEntity.properties.Last == true)
                {
                    if (lastEntity != null)
                        throw new Exception("First Line have to define only one time");
                    lastEntity = uEntity;
                }
            }

            isValidated = true;
        }

    }
}
