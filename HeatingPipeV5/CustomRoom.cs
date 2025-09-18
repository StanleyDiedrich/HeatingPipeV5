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
        public XYZ Origin { get; set; }

        public CustomRoom(Autodesk.Revit.DB.Document doc, Element element)
        {

            try
            {
                Element = element;
                var levelName = (element as SpatialElement).Level.Name;
                Level = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault(l => l.Name.Contains(levelName));



                //Level = (element as SpatialElement).Level;
                Name = element.LookupParameter("ADSK_Наименование квартиры").AsValueString();
                Number = element.LookupParameter("ADSK_Номер квартиры").AsValueString();
                
                XYZ center = ((element.Location) as LocationPoint).Point;
                Origin = new XYZ(center.X, center.Y, center.Z);
                Location = new UV(center.X, center.Y);
                SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
                var segments = (element as SpatialElement).GetBoundarySegments(opt);
                foreach (var segment in segments)
                {
                    CurveArray curveArray = new CurveArray();
                    foreach (BoundarySegment seg in segment)
                    {
                        Curve c = seg.GetCurve();
                        if (c != null || c.Length>0.1)
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
