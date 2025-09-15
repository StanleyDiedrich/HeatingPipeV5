using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HeatingPipeV5
{
    public class CustomLoop
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        public List<CustomBranch> Collection { get; set; }
        public int LoopNumber { get; set; }
        public CustomBranch SupplyBranch { get; set; }
        public CustomBranch ReturnBranch { get; set; }


        public CustomLoop (Autodesk.Revit.DB.Document doc, CustomCollection collection)
        {
            Collection = collection.Collection;
            SupplyBranch = new CustomBranch(doc);
            ReturnBranch = new CustomBranch(doc);
            

        }
        public CustomLoop(Autodesk.Revit.DB.Document doc)
        {
            SupplyBranch = new CustomBranch(doc);
            ReturnBranch = new CustomBranch(doc);
        }
        
    }
}
