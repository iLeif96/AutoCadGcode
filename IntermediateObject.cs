using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    /// <summary>
    /// Class representing Point3d after prevent validation.
    /// In this state they havn`t properties, only vertexes and type.
    /// </summary>
    public class IntermediateObject
    {
        public Type Type { get; set; }
        public List<Point3d> Vertexes { get; set; } = new List<Point3d>();
        public bool IsClockWise { get; set; } = false;
        public Point3d StartPoint
        {
            get
            {
                if (IsEmpty)
                    throw new Exception("Attempt to acccess to empty object");
                return Vertexes[0];
            }
        }
        public Point3d EndPoint
        {
            get
            {
                if (IsEmpty)
                    throw new Exception("Attempt to acccess to empty object");
                return Vertexes[Vertexes.Count - 1];
            }
        }

        public Point3d CenterPoint
        {
            get
            {
                if (IsEmpty || Type == null)
                    throw new Exception("Attempt to acccess to empty object");
                else if (Type != AvailableTypes.ARC)
                    throw new Exception("Attempt to acccess not to ARC");

                return Vertexes[1];
            }
        }

        public bool IsEmpty { get; private set; } = false;
        public IntermediateObject()
        {
            IsEmpty = true;
        }

        public IntermediateObject(Type type, Point3d[] vertexes, bool isClockWise = false)
        {
            if (CheckType(type, vertexes.Length))
            {
                IsClockWise = isClockWise;
                Type = type;
                if (vertexes != null)
                {
                    for (int i = 0; i < vertexes.Length; i++)
                        Vertexes.Add(vertexes[i]);
                }
            }
        }

        public IntermediateObject(Type type, List<Point3d> vertexes, bool isClockWise = false)
        {
            if (CheckType(type, vertexes.Count))
            {
                IsClockWise = isClockWise;
                Type = type;
                if (vertexes != null)
                    Vertexes = vertexes;
            }
        }

        private bool CheckType(Type type, int vertexesCount)
        {
            if (type != AvailableTypes.ARC && type != AvailableTypes.LINE)
                throw new Exception("This class can store only Arc and Line types");
            if (type == AvailableTypes.ARC && vertexesCount != 3)
                throw new Exception("This class can store 3 vertex for arc");
            else if (type == AvailableTypes.LINE && vertexesCount != 2)
                throw new Exception("This class can store maximum 2 vertex for line");
            
            return true;
        }

        public void Reverse()
        {
            Point3d _pt = Vertexes.Last<Point3d>();
            Vertexes[Vertexes.Count - 1] = Vertexes[0];
            Vertexes[0] = _pt;

            if (Type == AvailableTypes.ARC)
                IsClockWise = !IsClockWise;
        }

        /// <summary>
        /// Method checking is incoming point has touch any stored point or not;
        /// </summary>
        /// <param name="pt">Any 3d point that maybe connected to this obj</param>
        /// <returns>If point not touch object then return -1;
        /// If touch: return 0 for startPoint and 1 for endPoint</returns>
        public int IndexOfTouching(Point3d pt)
        {
            int index = -1;
            if ((Math.Abs(StartPoint.X) - Math.Abs(pt.X) < Global.HYSTERESIS) &&
                (Math.Abs(StartPoint.Y) - Math.Abs(pt.Y) < Global.HYSTERESIS))
                index = 0;
            else if ((Math.Abs(EndPoint.X) - Math.Abs(pt.X) < Global.HYSTERESIS) &&
                (Math.Abs(EndPoint.Y) - Math.Abs(pt.Y) < Global.HYSTERESIS))
                index = 1;

            return index;
        }
    }
}
