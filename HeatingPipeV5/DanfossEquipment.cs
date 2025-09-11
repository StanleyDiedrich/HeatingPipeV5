using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class DanfossEquipment
    {
        public string System { get; set; }
        public string Type { get; set; }
        public string Symbol { get; set; }
        public string n_L { get; set; } // из параметров
        public string Frg { get; set; }
        public string Size { get; set; }
        public string Cover { get; set; }
        public string LengthMax { get; set; }
        public string Alpha { get; set; }
        public string Connection { get; set; }

        public string Lvl { get; set; }
        public string Room { get; set; }
        public string Connection2 { get; set; }
        public string dT { get; set; } // пока по умолчанию
        public string Axo { get; set; }
        public string Status { get; set; } //sos
        public string Comment { get; set; } // Заполняем 
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }


        public DanfossEquipment (CustomElement element)
        {
            Id = element.ElementId.ToString();
            Comment = element.BranchMark.ToString();
            bool L_boolean = false;
            var parameters = element.Element.Parameters;


            foreach (Parameter parameter in parameters)
            {
                if (parameter.Definition.Name.Equals("L"))
                {
                    L_boolean = true;

                    break;
                }
            }
            if (L_boolean ==true)
            {
                n_L = element.Element.LookupParameter("L").AsValueString();
            }
            else
            {
                FamilyInstance instance = (element.Element as FamilyInstance);
                FamilySymbol symbol = instance.Symbol;
                n_L = symbol.LookupParameter("Длина прибора").AsValueString();
                
            }




            n_L = element.Element.LookupParameter("L").AsValueString();


            dT = "20";


        }
    }
}
