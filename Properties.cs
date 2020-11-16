using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace AutoCadGcode
{
    public class Properties
    {
        private static string _key = "KEY";
        private static int idInc = 0;
        public string KEY 
        {
            get
            {
                return Convert.ToString(PropertiesList[(int)Order.KEY]);
            }
            private set
            {
                PropertiesList[(int)Order.KEY] = _key;
            }
        }
        public bool Pumping
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Order.Pumping]);
            }
            set
            {
                PropertiesList[(int)Order.Pumping] = value;
            }
        }
        public bool First
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Order.First]);
            }
            set
            {
                PropertiesList[(int)Order.First] =  value;
            }
        }
        public bool Last
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Order.Last]);
            }
            set
            {
                PropertiesList[(int)Order.Last] = value;
            }
        }

        enum Order { KEY, Pumping, First, Last }

        public object[] PropertiesList = new object[Enum.GetValues(typeof(Order)).Length];


        public Properties(bool pump = false)
        {
            KEY = _key;
            Pumping = pump;
            First = false;
            Last = false;
        }

        public ResultBuffer ToBuffer()
        {
            ResultBuffer rb = new ResultBuffer();

            rb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, Convert.ToString(KEY)));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Convert.ToString(Pumping)));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Convert.ToString(First)));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Convert.ToString(Last)));          
                
            return rb;
        }

        public static Properties FromBuffer(ResultBuffer rb)
        {
            var arr = rb.AsArray();
            Properties props = new Properties();

            if (arr[0].Value as string != props.KEY)
                return null;


            props.Pumping = Convert.ToBoolean(arr[(int)Order.Pumping].Value);
            props.First = Convert.ToBoolean(arr[(int)Order.First].Value);
            props.Last = Convert.ToBoolean(arr[(int)Order.Last].Value);

            return props;
        }

        public override string ToString()
        {
            string str = "Properties: \n";

            for (int i = 0; i < this.PropertiesList.Length; i++)
                str = str + Enum.GetNames(typeof(Order))[i] + ": " + PropertiesList[i] + ";\n";

            return str;
        }
    }
}
