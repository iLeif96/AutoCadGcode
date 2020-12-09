using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    static class Templates
    {
        private const string TemplateNumber = "999999";
        public static class Commands
        {
            public static string StopAndPump(ref int index, int time, int power = 100)
            {
                string P = power >= 0 ? ToInstr("P", power) : "";

                string T = time >= 0 ? ToInstr("T", time / 100) : "";

                if (T == "")
                    throw new Exception("T in Stop and pump cant be empty");

                return BuildString(ref index, "G04", P, T);
            }

            public static string DisablePumping(ref int index, bool pumping)
            {
                string M = pumping ? "M10" : "M11";
                return BuildString(ref index, M);
            }
        }

        public static class Printing
        {
            public static string Line(ref int index, double x, double y, bool pumping, int speed = -1, int volume = -1)
            {
                string G = pumping ? "G01" : "G00";

                string F = speed >= 0 ? ToInstr("F", speed) : "";

                string V = (volume >= 0) && (pumping) ? ToInstr("V", volume) : "";

                return BuildString(ref index, G, ToInstr("X", x), ToInstr("Y", y), F, V);
            }

            public static string Arc(ref int index, double x, double y, double a, double b,
                bool isClockWise, bool pumping, int speed = -1, int volume = -1)
            {
                string G = isClockWise ? "G02" : "G03";

                string F = speed >= 0 ? ToInstr("F", speed) : "";

                string V = volume >= 0 ? ToInstr("V", volume) : "";

                string outString = "";

                //if (pumping == false)
                //    outString += Commands.SwitchPumping(ref index, false);

                outString += BuildString(ref index, G, ToInstr("X", x), ToInstr("Y", y),
                    ToInstr("A", a), ToInstr("B", b), F, V);

                //if (pumping == false)
                //    outString += Commands.SwitchPumping(ref index, true);

                return outString;
            }
        }

        private static string IndexToInstructionNumber(ref int index)
        {
            string outNum = "";
            int lenght = index.ToString().Length;
            if (TemplateNumber.Length > lenght)
            {
                for (int i = TemplateNumber.Length; i > lenght; i--)
                    outNum += "0";
            }
            index++;
            return "N" + outNum + index.ToString() + "0";
        }
        private static string BuildString(ref int index, params string[] strs)
        {
            string line = IndexToInstructionNumber(ref index);
            foreach (string str in strs)
                if (str != "")
                    line += " " + str;

            return line + "\n";
        }
        private static string ToInstr(params object[] objs)
        {
            string str = "";
            foreach (object obj in objs)
            {
                if (obj.GetType() == typeof(double))
                    str += Math.Round((double)obj, 4).ToString();
                else
                    str += obj.ToString();
            }

            return str;
        }
    }
}
