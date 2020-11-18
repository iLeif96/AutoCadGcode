using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    /// <summary>
    /// This class represent ready to transform to Gcode objects.
    /// The main idea is restruct diffrent types of AutoCad objects to a similar view.
    /// Every ValidatedObject has own type, but VertexData stored according to a single rule.
    /// Each object has to store vertexData for only one type and only one properties.
    /// It`s mean then we have lines with different pumping property we need interrupt VertexData
    /// and create next object. But if we have same type and properties, we can continiously add next vertexes
    /// </summary>
    public class ValidatedObject
    {
        public Type Type { get; set; }
        public Properties Properties { get; set; }
        public List<object> Vertexes { get; set; } = new List<object>();

        public ValidatedObject(Type type, Properties props, object vertex = null)
        {
            Type = type;
            Properties = props;
            if (vertex != null)
                Vertexes.Add(vertex);
        }

        public ValidatedObject(Type type, Properties props, object[] vertexes = null)
        {
            Type = type;
            Properties = props;
            if (vertexes != null)
            {
                for (int i = 0; i < vertexes.Length; i++)
                    Vertexes.Add(vertexes[0]);
            }
        }

        public ValidatedObject(Type type, Properties props, List<object> vertexes = null)
        {
            Type = type;
            Properties = props;
            if (vertexes != null)
                Vertexes = vertexes;
        }

        public bool AddVertex(Type type, Properties props, object vertex)
        {
            if (type != Type || props != Properties)
                return false;

            Vertexes.Add(vertex);
            return true;
        }
    }
}
