using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    public class GcodeGenerator
    {
        public List<string> GcodeLines = new List<string>();

        private string FileName = "gcode";

        public GcodeGenerator(string fileName)
        {
            FileName = fileName;
        }

        public bool GenerateGcode(List<ValidatedObject> vList, ValidatedObject firstLine)
        {
            if (vList.Count < 1)
                throw new Exception("List of Valid objects can`t be empty");

            GcodeLines.Clear();

            Dictionary<CommandType, ValidProperties> continiouslyCommands = new Dictionary<CommandType, ValidProperties>() 
            {
                { CommandType.DisablePumping, new ValidProperties(CommandType.DisablePumping) { DisablePumping = false } }
            };

            int lineNumber = 0;

            //set first segment

            string line = StringForFirstPoint(vList.First(), ref lineNumber);
            if (line == "")
                throw new Exception("Null object in Valid objecs list");
            GcodeLines.Add(line);

            foreach (ValidatedObject vObj in vList)
            {
                if (vObj == null)
                    throw new Exception("Null object in Valid objecs list");

                if (vObj.IsCommand)
                {
                    line = StringForCommand(vObj, ref lineNumber);
                    if (line == "")
                        throw new Exception("Returned empty line");

                    foreach (ValidProperties cmd in continiouslyCommands.Values)
                    {
                        if (cmd.CommandType == vObj.Properties.CommandType)
                            if (cmd.CommandType == CommandType.DisablePumping)
                            {
                                if (vObj.Properties.DisablePumping != cmd.DisablePumping)
                                    cmd.DisablePumping = !cmd.DisablePumping;
                            }
                    }
                    
                    GcodeLines.Add(line);
                }
                else if (vObj.Type == AvailableTypes.LINE)
                {
                    line = StringForLine(vObj, ref lineNumber);
                    if (line == "")
                        throw new Exception("Returned empty line");

                    GcodeLines.Add(line);
                }
                else if (vObj.Type == AvailableTypes.ARC)
                {
                    if (!vObj.Properties.Pumping && !continiouslyCommands[CommandType.DisablePumping].DisablePumping)
                    {
                        line = StringForCommand(new ValidatedObject(new ValidProperties(
                            CommandType.DisablePumping) { DisablePumping = true }), ref lineNumber);
                        if (line == "")
                            throw new Exception("Returned empty line");
                        GcodeLines.Add(line);

                        line = StringForArc(vObj, ref lineNumber);
                        if (line == "")
                            throw new Exception("Returned empty line");
                        GcodeLines.Add(line);

                        line = StringForCommand(new ValidatedObject(new ValidProperties(
                            CommandType.DisablePumping) { DisablePumping = false }), ref lineNumber);
                        if (line == "")
                            throw new Exception("Returned empty line");
                        GcodeLines.Add(line);
                    }
                    else
                    {
                        line = StringForArc(vObj, ref lineNumber);
                        if (line == "")
                            throw new Exception("Returned empty line");
                        GcodeLines.Add(line);

                    }
                }
            }
            return true;
        }

        public string GcodeToFile(string fileName = null)
        {
            if (fileName == null)
                fileName = FileName;

            fileName = Path.GetFileNameWithoutExtension(fileName);
            try {
                
                if (GcodeLines.Count == 0)
                    throw new Exception("There are no gcode lines for whriting to file");

                
                if (!Directory.Exists(@"Files"))
                    Directory.CreateDirectory(@"Files");

                fileName = @"Files\" + fileName;
                while (File.Exists(fileName))
                    fileName += DateTime.Now.TimeOfDay.ToString();
                    

                using (StreamWriter sW = new StreamWriter(
                    new FileStream(fileName + ".gcode" , FileMode.Create, FileAccess.Write)))
                {
                    foreach (string line in GcodeLines)
                    {
                        sW.Write(line);
                    }
                }
            }
            catch (Exception e)
            {
                Global.Editor.WriteMessage("Error until creatig gcode file: " + e.ToString());
            }
            return fileName;
        }

        private string StringForFirstPoint(ValidatedObject vObj, ref int index)
        {
            if (vObj.Vertexes.Count == 2)
                return Templates.Printing.Line(ref index, vObj.StartPoint.X, vObj.StartPoint.Y, false);
            return "";
        }
        private string StringForCommand(ValidatedObject vObj, ref int index)
        {
            if (vObj.IsCommand)
            {
                switch (vObj.Properties.CommandType)
                {
                    case CommandType.None:
                        return "";
                    case CommandType.StopAndPump:
                        return Templates.Commands.StopAndPump(ref index, vObj.Properties.StopAndPump);
                    case CommandType.DisablePumping:
                        return Templates.Commands.DisablePumping(ref index, vObj.Properties.DisablePumping);
                }
            }
            return "";
        }
        private string StringForLine(ValidatedObject vObj, ref int index)
        {
            if (vObj.Type == AvailableTypes.LINE)
            {
                if (vObj.Vertexes.Count == 2)
                    return Templates.Printing.Line(ref index, vObj.EndPoint.X, vObj.EndPoint.Y, vObj.Properties.Pumping);
            }
            return "";
        }
        private string StringForArc(ValidatedObject vObj, ref int index)
        {
            if (vObj.Type == AvailableTypes.ARC)
            {
                if (vObj.Vertexes.Count == 3)
                    return Templates.Printing.Arc(ref index, vObj.EndPoint.X, vObj.EndPoint.Y,
                        vObj.CenterPoint.X, vObj.CenterPoint.Y, vObj.IsClockWise, vObj.Properties.Pumping);
            }
            return "";
        }
    }

    static class Templates
    {
        private const string TemplateNumber = "999999";
        public static class Commands {
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

                string V = volume >= 0 ? ToInstr("V", volume) : "";

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
