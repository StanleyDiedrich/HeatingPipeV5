using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class DanfossManifold
    {
        public string Id { get; set; }
        public string System { get; set; }
        public string Type { get; set; }
        public string Symbol { get; set; }
        public string Dn { get; set; }
        public string Tmix { get; set; }
        public string dT { get; set; }
        public string Contours { get; set; }
        public string Lvl { get; set; }
        public string Lvl2 { get; set; }
        public string Axo { get; set; }
        public string Condition { get; set; }
        public string Comment { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }

        public DanfossManifold(CustomElement element)
        {
            Id = element.ElementId.ToString();
            FamilyInstance familyInstance = element.Element as FamilyInstance;
            MEPModel mepModel = familyInstance.MEPModel;
            ConnectorSet connectorSet = mepModel.ConnectorManager.Connectors;

            int contours = 0;
            foreach (Connector connector in connectorSet)
            {
                contours++;
            }
            contours = (contours - 2) / 2;
            Contours = contours.ToString();
            Comment = element.BranchMark.Split('_')[0]+"К";
        }

    }
}
