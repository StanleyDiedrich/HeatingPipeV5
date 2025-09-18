using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
   public class CustomRoom
    {
        public Element Element { get; set; }
        public Level Level { get; set; }
        public UV Location { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string MepSpaceName { get; set; }
        public string HeatLoading { get; set; }
        public string Temperature { get; set; }
        public Solid CSolid { get; set; }
        public List<CurveArray> BoundarySegments { get; set; } = new List<CurveArray>();


        public CustomRoom(Autodesk.Revit.DB.Document doc, Element element)
        {

            try
            {
                Element = element;
                Level = doc.GetElement(element.LevelId) as Level;
                Name = element.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString();
                Number = element.LookupParameter("ADSK_Номер квартиры").AsValueString();
                HeatLoading = element.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).AsValueString().Split()[0];
                Temperature = element.LookupParameter("ADSK_Температура в помещении").AsValueString().Split()[0];
                XYZ center = ((element.Location) as LocationPoint).Point;
                Location = new UV(center.X, center.Y);
                SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
                var segments = (element as SpatialElement).GetBoundarySegments(opt);
                foreach (var segment in segments)
                {
                    CurveArray curveArray = new CurveArray();
                    foreach (BoundarySegment seg in segment)
                    {
                        Curve c = seg.GetCurve();
                        if (c != null)
                        {
                            curveArray.Append(c);
                        }

                    }
                    BoundarySegments.Add(curveArray);
                }
            }
            catch
            {

            }





        }
    }
}
