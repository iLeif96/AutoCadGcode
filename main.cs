
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace AutoCadGcode
{
    public class Main : IExtensionApplication
    {
        public delegate void DocumentLoadedHandler(List<Entity> list = null);
        public static event DocumentLoadedHandler DocumentLoadedEvent;

        public void Initialize()
        {
            Process.Greetings();
            Process.Start();
            DocumentLoadedEvent?.Invoke();
        }

        public void Terminate()
        {
            //
        }
    }
}