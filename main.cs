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
    public class Global
    {
        public static Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;

        public static Database dB = Application.DocumentManager.MdiActiveDocument.Database;

        public static Autodesk.AutoCAD.DatabaseServices.TransactionManager tM = dB.TransactionManager;

        public static Document doc = Application.DocumentManager.MdiActiveDocument;

        public static Dictionary<ObjectId, UserEntity> uEntitys = new Dictionary<ObjectId, UserEntity>();

        public static Process process = new Process();

        public static GUI gui = new GUI();

    }
}