using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;

namespace HeatingPipeV5
{
    public class CustomPipe
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Temperature { get; set; }
        public double Density { get; set; }
        public double Viscosity { get; set; }
        public double Reynolds { get; set; }
        public double Diameter { get; set; }
        public double FlowVelocity { get; set; }
        public double Roughness { get; set; }
        public double Lambda { get; set; }
        public double Pressure { get; set; }

        public CustomPipe (Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element.Element;
            ElementId = element.ElementId;

            ElementId syselementId = (Element as MEPCurve).MEPSystem.GetTypeId();

            Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);
            FlowVelocity = Convert.ToDouble(Element.get_Parameter(BuiltInParameter.RBS_PIPE_VELOCITY_PARAM).AsValueString().Split()[0]);
            Roughness = Element.get_Parameter(BuiltInParameter.RBS_PIPE_ROUGHNESS_PARAM).AsDouble() * 304.8;
            Diameter = Convert.ToDouble(Element.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsValueString());
            Viscosity visosity = new Viscosity();
            Viscosity = visosity.GetViscosity(Temperature);
            CustomDensity density = new CustomDensity();
            Density = density.GetDensity(Temperature);
            Reynolds = FlowVelocity * Diameter / Viscosity;
            Lambda = GetLambda(Reynolds, Roughness);
            Pressure = Lambda / Diameter * Density * FlowVelocity * FlowVelocity / 2;
        }

        private double GetLambda(double reynolds, double roughness)
        {
            Lambda = 0.11 * Math.Pow((68 / reynolds + roughness / Diameter), 0.25);
            return Lambda;
        }
    }
}
