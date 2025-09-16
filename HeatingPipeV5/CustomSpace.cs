using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.Windows.Controls;

namespace HeatingPipeV5
{
    public class CustomSpace
    {
        public Element Element { get; set; }
        public Level Level { get; set; }
        public UV Location { get; set; }
        public string Name { get; set; }
        public string HeatLoading { get; set; }
        public string Temperature { get; set; }
        public Solid CSolid { get; set; }


        public CustomSpace(Autodesk.Revit.DB.Document doc, Element element)
        {
            
            try
            {
                Element = element;
                Level = doc.GetElement(element.LevelId) as Level;

                Name = element.LookupParameter("ADSK_Номер квартиры").AsValueString();
                HeatLoading = element.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).AsValueString().Split()[0];
                Temperature = element.LookupParameter("ADSK_Температура в помещении").AsValueString().Split()[0];
                XYZ center = ((element.Location) as LocationPoint).Point;
                Location = new UV(center.X, center.Y);
            }
            catch
            {

            }
            
            
           

            
        }
    }
}