﻿using System;
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
        public double Length { get; set; }
        public string Volume { get; set; }
        

        public CustomPipe (Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element.Element;
            ElementId = element.ElementId;

            ElementId syselementId = (Element as MEPCurve).MEPSystem.GetTypeId();

            Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);
            Volume = Convert.ToString(Math.Round(Element.get_Parameter(BuiltInParameter.RBS_PIPE_FLOW_PARAM).AsDouble() * 102,2));
            FlowVelocity = Convert.ToDouble(Element.get_Parameter(BuiltInParameter.RBS_PIPE_VELOCITY_PARAM).AsValueString().Split()[0]);
            Roughness = Element.get_Parameter(BuiltInParameter.RBS_PIPE_ROUGHNESS_PARAM).AsDouble() * 304.8;
            Diameter = Convert.ToDouble(Element.get_Parameter(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM).AsValueString())/1000;
            Length = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * 304.8/1000;
            Viscosity visosity = new Viscosity();
            Viscosity = visosity.GetViscosity(Temperature);
            CustomDensity density = new CustomDensity();
            Density = density.GetDensity(Temperature);
            Reynolds = FlowVelocity * Diameter * 1000000 / Viscosity;

            Lambda = GetLambda(Reynolds, Roughness);
            Pressure = Lambda / Diameter * Density * FlowVelocity * FlowVelocity / 2*Length;
            if (double.IsInfinity(Pressure))
            {
                Pressure = 0;
            }
        }

        private double GetLambda(double reynolds, double roughness)
        {
            if (reynolds<2300)
            {
                Lambda = 0.11 * Math.Pow((68 / reynolds + roughness / Diameter), 0.25);
            }
            else
            {
                double x = Math.Pow(-2 * Math.Log10((Roughness / 3.7 * Diameter + 2.51 / (Reynolds * Math.Sqrt(1)))), -2);
               // double x = Math.Pow((-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(1) + (roughness) / (3.7 * Diameter))), 2);
                double y = 0;
                double y1 = 0;



                do
                {
                    y = x;
                    x = Math.Pow(-2 * Math.Log10((Roughness / 3.7 * Diameter + 2.51 / (Reynolds * Math.Sqrt(y)))), -2);
                }
                while (Math.Abs(y - x) > 0.0000001);
                Lambda = x;
                // Lambda = y;
               /*double x = (-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(1)) + (roughness ) / (3.7 * diameter));
               double x1 = Math.Pow(x, -2.0);
               double x2 = (-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(x1)) + (roughness ) / (3.7 * diameter));
               double x3 = Math.Pow(x2, -2.0);
               double x4 = (-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(x3)) + (roughness ) / (3.7 * diameter));
               double x5 = Math.Pow(x3, -2.0);
               double x6 = (-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(x5)) + (roughness ) / (3.7 * diameter));
               double x7 = Math.Pow(x6, -2.0);
               double x8 = (-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(x7)) + (roughness ) / (3.7 * diameter));
               double x9 = Math.Pow(x8, -2.0);
               double x10 = (-2 * Math.Log10(2.51 / reynolds * Math.Sqrt(x9)) + (roughness ) / (3.7 * diameter));
               double x11 = Math.Pow(x10, -2.0);
               Lambda = x11;*/
               // Lamda= (-2 * LOG10(2, 51 / ([@24] * КОРЕНЬ(1)) + ([@13] / 1000) / (3, 7 * ([@18] / 1000)))) ^ -2

              /* double f = 0.02;
                double fOld;

                do
                {
                    fOld = f;
                    // Вычисляем f по уравнению Коулбрука
                    f = 1.0 / Math.Pow((-2.0 * Math.Log10((Roughness / Diameter) / 3.7 + 5.74 / Math.Pow(Reynolds, 0.9))), 2);
                } 
                while (Math.Abs(f - fOld) > 1e-6);
                Lambda = f;*/


            }
           
            return Lambda;
        }
    }
}
