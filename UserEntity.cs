using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    public class UserEntity
    {
        public delegate void WrongTypeDetectedHandler(ObjectId objectId);
        public static event WrongTypeDetectedHandler WrongTypeDetectedEvent;
        public Entity Entity { get; set; }
        public Properties Properties { get; set; }
        public Type Type { get { return Entity.GetType(); } }
        public ObjectId ObjectId { get; set; }
        public Line AsLine { get { return (Line)Entity; } }
        public Arc AsArc { get { return (Arc)Entity; } }
        public Polyline AsPoly { get { return (Polyline)Entity; } }
        public Point3d StartPoint
        {
            get
            {
                if (!isNeedReverse)
                    return (Entity as Curve).StartPoint;
                else
                    return (Entity as Curve).EndPoint;
            }
        }
        public Point3d EndPoint
        {
            get
            {
                if (!isNeedReverse)
                    return (Entity as Curve).EndPoint;
                else
                    return (Entity as Curve).StartPoint;
            }
        }

        /// <summary>
        ///Using then we need to create validated objects.
        ///Influence to StartPoint and EndPoint returning values (reverse them)
        /// </summary>
        private bool isNeedReverse = false;

        private UserEntity(Entity entity, Properties properties)
        {
            this.Entity = entity;
            this.Properties = properties;
            this.ObjectId = entity.ObjectId;
        }
        public static UserEntity Create(Entity entity, Properties properties)
        {
            try
            {
                if(CheckType(entity))
                    return new UserEntity(entity, properties);
            }
            catch (Exception e)
            {
                Global.Editor.WriteMessage(e.ToString());
            }
            return null;
        }
        public static bool CheckType(Entity entity)
        {
            bool result = false;
            if (entity == null)
                return result;

            Type type = entity.GetType();
            if (type ==  AvailableTypes.LINE || type == AvailableTypes.POLYLINE || type == AvailableTypes.ARC)
                result = true;
            else if (type == typeof(Polyline2d) || type == typeof(Polyline3d))
                Global.Editor.WriteMessage("ERROR. You have to convert polyline2d or polyline3d to polyline\n");
            else
                Global.Editor.WriteMessage("User entity can be only Arc, Line or Polyline\n");

            //throw new Exception("User entity can be only Arc, Line or Polyline");

            if (!result)
                WrongTypeDetectedEvent?.Invoke(entity.ObjectId);

            return result;
        }

        public List<IntermediateObject> ToIntermediateList()
        {
            List<IntermediateObject> list = new List<IntermediateObject>();
            Type type = Entity.GetType();

            if (type == AvailableTypes.POLYLINE)
            {
                Polyline polyline = Entity as Polyline;

                LineSegment3d line;
                CircularArc2d arc;

                if (!isNeedReverse)
                {
                    for (int i = 0; i < polyline.NumberOfVertices; i++)
                    {
                        if (polyline.GetSegmentType(i) == SegmentType.Arc)
                        {
                            arc = polyline.GetArcSegment2dAt(i);
                            list.Add(new IntermediateObject(typeof(Arc), new Point3d[]
                                { To3d(arc.StartPoint), To3d(arc.Center), To3d(arc.EndPoint) }, arc.IsClockWise));
                        }
                        if (polyline.GetSegmentType(i) == SegmentType.Line)
                        {
                            line = polyline.GetLineSegmentAt(i);
                            list.Add(new IntermediateObject(typeof(Line),
                                new Point3d[] { line.StartPoint, line.EndPoint }));
                        }
                    }
                } else
                {
                    for (int i = polyline.NumberOfVertices - 1; i >= 0; i--)
                    {
                        if (polyline.GetSegmentType(i) == SegmentType.Arc)
                        {
                            arc = polyline.GetArcSegment2dAt(i);
                            list.Add(new IntermediateObject(typeof(Arc), new Point3d[]
                                { To3d(arc.EndPoint), To3d(arc.Center), To3d(arc.StartPoint) }, !arc.IsClockWise));
                        }
                        if (polyline.GetSegmentType(i) == SegmentType.Line)
                        {
                            line = polyline.GetLineSegmentAt(i);
                            list.Add(new IntermediateObject(typeof(Line),
                                new Point3d[] { line.EndPoint, line.StartPoint }));
                        }
                    }
                }
            }
            else
                throw new Exception("This method can work only with polyline\n");

            return list;
        }

        public UserEntity ConvertToPolyline()
        {
            bool result = CheckType(Entity);

            if (this.Entity.GetType() != AvailableTypes.POLYLINE)
            {
                result = false;
                if (this.Entity.GetType() == AvailableTypes.ARC)
                {
                    Arc arc = this.Entity as Arc;
                    Polyline polyline = new Polyline();
                    polyline.AddVertexAt(0, new Point2d(arc.StartPoint.X, arc.StartPoint.Y),
                        GetArcBulge(arc), 0, 0);
                    polyline.AddVertexAt(1, new Point2d(arc.EndPoint.X, arc.EndPoint.Y), 0, 0, 0);
                    this.Entity = polyline;
                    result = true;
                }
                else if (this.Entity.GetType() == AvailableTypes.LINE)
                {
                    Line line = this.Entity as Line;
                    Polyline polyline = new Polyline();
                    polyline.AddVertexAt(0, new Point2d(line.StartPoint.X, line.StartPoint.Y), 0, 0, 0);
                    polyline.AddVertexAt(1, new Point2d(line.EndPoint.X, line.EndPoint.Y), 0, 0, 0);
                    this.Entity = polyline;
                    result = true;

                }
            }

            if (result == true)
                return this;
            else
                return null;
        }

        /// <summary>
        /// Check touching incoming point to Start or End point of entity
        /// </summary>
        /// <param name="pt">Any 3d point</param>
        /// <returns>-1: not touching; 0: touch first point; 1: touch end point</returns>
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

        public void MarkToReverse()
        {
            isNeedReverse = true;
        }

        public void UnmarkToReverse()
        {
            isNeedReverse = false;
        }

        private static double GetArcBulge(Arc arc)
        {
            double deltaAng = arc.EndAngle - arc.StartAngle;
            if (deltaAng < 0)
                deltaAng += 2 * Math.PI;
            return Math.Tan(deltaAng * 0.25);
        }

        private static Point3d To3d(Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }
    }
}
