using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Interfaces.Streaming;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;


namespace AutoCadGcode
{
    public struct AvailableTypes
    {
        public static Type LINE { get { return typeof(Line); } }
        public static Type ARC { get { return typeof(Arc); } }
        public static Type POLYLINE { get { return typeof(Polyline); } }
    }

    public class Global : IExtensionApplication
    {
        /// <summary>
        /// Using for approximate autocad Points coordinates
        /// </summary>
        public const double HYSTERESIS = 0.01;

        public delegate void DocumentLoadedHandler(List<Entity> list = null);
        public static event DocumentLoadedHandler DocumentLoadedEvent;

        public static Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;

        public static Database dB = Application.DocumentManager.MdiActiveDocument.Database;

        public static Autodesk.AutoCAD.DatabaseServices.TransactionManager tM = dB.TransactionManager;

        public static Document doc = Application.DocumentManager.MdiActiveDocument;

        public void Initialize()
        {
            Global.editor.WriteMessage("AutoCadGcode by Svetoslav Elkin\n");

            Process.Start();
            DocumentLoadedEvent?.Invoke();
        }

        public void Terminate()
        {
            //
        }
    }
}