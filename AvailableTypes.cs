using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCadGcode
{
    public struct AvailableTypes
    {
        public static Type LINE { get { return typeof(Line); } }
        public static Type ARC { get { return typeof(Arc); } }
        public static Type POLYLINE { get { return typeof(Polyline); } }
    }
}
