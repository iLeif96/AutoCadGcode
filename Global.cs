using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutoCadGcode
{
    public class Global
    {
        /// <summary>
        /// Using for approximate autocad Points coordinates
        /// </summary>
        public const double HYSTERESIS = 0.01;

        public static DocumentCollection DocumentManager = Application.DocumentManager;

        public static Editor Editor = Application.DocumentManager.MdiActiveDocument.Editor;

        public static Database DB = Application.DocumentManager.MdiActiveDocument.Database;

        public static Autodesk.AutoCAD.DatabaseServices.TransactionManager TM = DB.TransactionManager;

        public static Document Doc = Application.DocumentManager.MdiActiveDocument;
    }
}
