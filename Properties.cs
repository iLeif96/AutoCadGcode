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
        private static string _key = "version_2";
        private static int idInc = 0;
        public string KEY 
        {
            get
            {
                return Convert.ToString(PropertiesList[(int)Fields.KEY]);
            }
            private set
            {
                PropertiesList[(int)Fields.KEY] = _key;
            }
        }
        public bool Pumping
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Fields.Pumping]);
            }
            set
            {
                PropertiesList[(int)Fields.Pumping] = value;
            }
        }

        public bool Printable
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Fields.Printable]);
            }
            set
            {
                if (value == false)
                {
                    Order = -1;
                }
                else
                {
                    First = false;
                    Last = false;
                }

                    PropertiesList[(int)Fields.Printable] = value;
            }
        }

        public int Order
        {
            get
            {
                return Convert.ToInt32(PropertiesList[(int)Fields.Order]);
            }
            set
            {
                PropertiesList[(int)Fields.Order] = value;
            }
        }
        public bool First
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Fields.First]);
            }
            set
            {
                if (value == true)
                    Last = false;
                PropertiesList[(int)Fields.First] = value;
            }
        }
        public bool Last
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Fields.Last]);
            }
            set
            {
                if (value == true)
                    First = false;
                PropertiesList[(int)Fields.Last] = value;
            }
        }

        enum Fields { KEY, Pumping, Printable, Order, First, Last }

        public object[] PropertiesList = new object[Enum.GetValues(typeof(Fields)).Length];


        public Properties(bool pump = false)
        {
            KEY = _key;
            Pumping = pump;
            Printable = true;
            Order = 0;
            First = false;
            Last = false;
        }

        public ResultBuffer ToBuffer()
        {
            ResultBuffer rb = new ResultBuffer();

            rb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, Convert.ToString(KEY)));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Convert.ToString(Pumping)));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Convert.ToString(Printable)));
            rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, Convert.ToString(Order)));
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


            props.Pumping = Convert.ToBoolean(arr[(int)Fields.Pumping].Value);
            props.Printable = Convert.ToBoolean(arr[(int)Fields.Printable].Value);
            props.Order = Convert.ToInt32(arr[(int)Fields.Order].Value);
            props.First = Convert.ToBoolean(arr[(int)Fields.First].Value);
            props.Last = Convert.ToBoolean(arr[(int)Fields.Last].Value);

            return props;
        }

        public override string ToString()
        {
            string str = "Properties: \n";

            for (int i = 0; i < this.PropertiesList.Length; i++)
                str = str + Enum.GetNames(typeof(Fields))[i] + ": " + PropertiesList[i] + ";\n";
            
            return str;
        }
    }
}
