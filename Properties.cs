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
        private static readonly string _key = "version_2.2";

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
                    Command = false;
                    First = false;
                    Last = false;
                    StopAndPump = 0;
                }

                    PropertiesList[(int)Fields.Printable] = value;
            }
        }
        public bool Command
        {
            get
            {
                return Convert.ToBoolean(PropertiesList[(int)Fields.Command]);
            }
            set
            {
                PropertiesList[(int)Fields.Command] = value;
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
                if (value > 0 && Printable == false)
                    return;
                PropertiesList[(int)Fields.Order] = value;
            }
        }
        public int StopAndPump
        {
            get
            {
                return Convert.ToInt32(PropertiesList[(int)Fields.StopAndPump]);
            }
            set
            {
                PropertiesList[(int)Fields.StopAndPump] = value;
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
                if (Printable == true)
                    return;
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
                if (Printable == true)
                    return;
                if (value == true)
                    First = false;
                PropertiesList[(int)Fields.Last] = value;
            }
        }


        enum Fields { KEY, Pumping, Printable, Order, Command, StopAndPump, First, Last }

        public object[] PropertiesList = new object[Enum.GetValues(typeof(Fields)).Length];


        public Properties(bool pump = false)
        {
            KEY = _key;
            Pumping = pump;
            Printable = true;
            Order = 0;
            Command = false;
            StopAndPump = 0;
            First = false;
            Last = false;
        }

        public ResultBuffer ToBuffer()
        {
            ResultBuffer rb = new ResultBuffer();

            rb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, Convert.ToString(KEY)));
            for (int i = (int)Fields.KEY + 1; i < PropertiesList.Length; i++)
                rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString,
                    Convert.ToString(PropertiesList[i])));         
                
            return rb;
        }

        public static Properties FromBuffer(ResultBuffer rb)
        {
            var arr = rb.AsArray();
            Properties props = new Properties();

            if (arr[0].Value as string != props.KEY)
                return null;

            Type type;
            for (int i = (int)(Fields.KEY + 1); i < props.PropertiesList.Length; i++)
            {
                type = props.PropertiesList[i].GetType();

                if (type == typeof(int))
                    props.PropertiesList[i] = Convert.ToInt32(arr[i].Value);
                else if (type == typeof(bool))
                    props.PropertiesList[i] = Convert.ToBoolean(arr[i].Value);
                else if (type == typeof(double))
                    props.PropertiesList[i] = Convert.ToDouble(arr[i].Value);
                else if (type == typeof(string))
                    props.PropertiesList[i] = Convert.ToString(arr[i].Value);
                else
                    throw new Exception("Unresolved type in properties");

                //type = props.PropertiesList[i].GetType();
                //props.PropertiesList[i] =
                //    type.GetMethod("Parse").Invoke(props.PropertiesList[i], new object[] { arr[i].Value });
            }

            //props.Pumping = Convert.ToBoolean(arr[(int)Fields.Pumping].Value);
            //props.Printable = Convert.ToBoolean(arr[(int)Fields.Printable].Value);
            //props.Order = Convert.ToInt32(arr[(int)Fields.Order].Value);
            //props.Command = Convert.ToBoolean(arr[(int)Fields.Command].Value);
            //props.StopAndPump = Convert.ToInt32(arr[(int)Fields.StopAndPump].Value);
            //props.First = Convert.ToBoolean(arr[(int)Fields.First].Value);
            //props.Last = Convert.ToBoolean(arr[(int)Fields.Last].Value);

            return props;
        }

        public Properties Clone()
        {
            Properties props = new Properties();
            props.PropertiesList = this.PropertiesList.Clone() as object[];
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
