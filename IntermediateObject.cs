using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
    /// <summary>
    /// Class representing objects after prevent validation.
    /// In this state they havn`t properties, only vertexes and type.
    /// </summary>
    public class IntermediateObject
    {
        public Type Type { get; set; }
        public List<object> Vertexes { get; set; } = new List<object>();

        public IntermediateObject(Type type, object vertex = null)
        {
            Type = type;
            if (vertex != null)
                Vertexes.Add(vertex);
        }

        public IntermediateObject(Type type, object[] vertexes = null)
        {
            Type = type;
            if (vertexes != null)
            {
                for (int i = 0; i < vertexes.Length; i++)
                    Vertexes.Add(vertexes[0]);
            }
        }

        public IntermediateObject(Type type, List<object> vertexes = null)
        {
            Type = type;
            if (vertexes != null)
                Vertexes = vertexes;
        }

        public virtual bool AddVertex(Type type, object vertex)
        {
            if (type != Type)
                return false;

            Vertexes.Add(vertex);
            return true;
        }
    }
}
