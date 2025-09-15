using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class Worksheet
    {
        public string WorksheetName { get; set; }
        public bool IsSelected { get; set; }
        public Workset Workset { get; set; }

        public Worksheet(string worksheetName, Workset workset)
        {
            WorksheetName = worksheetName;
            Workset = workset;
        }
    }
}
