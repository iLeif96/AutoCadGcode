using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    /// <summary>
    /// This class represent ready to transform to Gcode Point3ds.
    /// The main idea is restruct diffrent types of AutoCad Point3ds to a similar view.
    /// Every ValidatedObject has own type, but VertexData stored according to a single rule.
    /// Each Point3d has to store vertexData for only one type and only one properties.
    /// It`s mean then we have lines with different pumping property we need interrupt VertexData
    /// and create next Point3d. But if we have same type and properties, we can continiously add next vertexes
    /// </summary>
    public class ValidatedObject : IntermediateObject
    {
        public ValidProperties Properties { get; set; }
        public bool IsCommand { get; private set; } = false;
        public ValidatedObject(ValidProperties props)
        {
            Properties = props;
            IsCommand = true;
        }

        public ValidatedObject(IntermediateObject iObj, ValidProperties props)
        : base (iObj.Type, iObj.Vertexes, iObj.IsClockWise)
        {
            Properties = props;
        }

        public ValidatedObject(Type type, ValidProperties props, Point3d[] vertexes, bool isClockWise = false)
            : base(type, vertexes, isClockWise)
        {
            Properties = props;
        }

        public ValidatedObject(Type type, ValidProperties props, List<Point3d> vertexes, bool isClockWise = false)
            : base(type, vertexes, isClockWise)
        {
            Properties = props;
        }
    }
}
