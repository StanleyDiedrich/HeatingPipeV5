using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using HeatingPipeV5;
using System.Net;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Runtime.Remoting.Contexts;

namespace HeatingPipeV5
{
    public class CustomTee
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double Diameter { get; set; }
        public double ElbowRadius { get; set; }
        public XYZ LocPoint { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public CustomConnector OutletConnector1 { get; set; }
        public CustomConnector OutletConnector2 { get; set; }

        public List<CustomConnector> OutletConnectors { get; set; } = new List<CustomConnector>();
        public PipeSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public double RelA { get; set; }
        public double Angle { get; set; }
        public double Velocity { get; set; }
        public double PDyn { get; set; }
        public double Density { get; set; }
        public double Temperature { get; set; }
        public CustomTee(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            
            Document = document;
            if (element.Element is Pipe)
            {
                element.DetailType = CustomElement.Detail.Pipe;
                CustomPipe customPipe = new CustomPipe(Document, element);
                element.Ptot = customPipe.Pressure;
                return;
            }
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;
            

           /* Parameter dn1 = element.Element.LookupParameter("Условный диаметр_1");
            Parameter dn2 = element.Element.LookupParameter("Условный диаметр_2");
            Parameter dn3 = element.Element.LookupParameter("Условный диаметр_1");

            if (dn1 == null)
            {
                foreach (Parameter parameter in Element.Element.Parameters)
                {
                    if (parameter.Id.IntegerValue == 2302147)
                    {
                        dn1 = parameter;
                    }
                }
            }

            if (dn2 ==null)
            {
                foreach (Parameter parameter in Element.Element.Parameters)
                {
                      if (parameter.Id.IntegerValue == 2302162 )
                    {
                        dn2 = parameter;
                    }
                }
            }
            if (dn3 == null)
            {
                foreach (Parameter parameter in Element.Element.Parameters)
                {
                    if (parameter.Id.IntegerValue == 2302163)
                    {
                        dn3 = parameter;
                    }
                }
            }*/
            






            //double length = 0;
          /*  double diameter1 = Convert.ToDouble(dn1.AsValueString());
            double diameter3 = Convert.ToDouble(dn2.AsValueString());
            double diameter = Convert.ToDouble(dn3.AsValueString());*/


            if (document.GetElement(ElementId) is FamilyInstance)
            {
                ElementId syselementId = null;
                LocPoint = ((element.Element.Location) as LocationPoint).Point;
                foreach (Connector connector in Element.OwnConnectors)
                {
                    if (connector.Domain == Domain.DomainUndefined)
                    {
                        continue;
                    }
                    else
                    {
                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain == Domain.DomainUndefined)
                            {
                                continue;
                            }
                            else
                            {
                                SystemType = connect.PipeSystemType;
                                CustomConnector custom = new CustomConnector(Document, ElementId, SystemType);

                                if (Document.GetElement(connect.Owner.Id) is PipingSystem || Document.GetElement(connect.Owner.Id) is PipeInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                /*else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }*/


                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {

                                    if (SystemType == PipeSystemType.SupplyHydronic)
                                    {

                                        if (connect.Direction == FlowDirectionType.Out)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 3600);
                                            }
                                           /* else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            try
                                            {
                                                syselementId = (connect.MEPSystem as PipingSystem).GetTypeId();
                                                Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);
                                            
                                            }
                                            catch
                                            { }
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 3600);

                                            }
                                           /* else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                        }



                                    }
                                    else if (SystemType == PipeSystemType.ReturnHydronic)
                                    {
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 3600);

                                            }
                                           /* else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            try
                                            {
                                                syselementId = (connect.MEPSystem as PipingSystem).GetTypeId();
                                                Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);
                                            }
                                            catch
                                            { }
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 3600);

                                            }
                                            /*else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

                                        }
                                    }

                                }

                            }

                        }
                    }

                }
            }
            double maxflow = double.MinValue;
            CustomConnector selectedconnector = null; // Initialize to null

            OutletConnector1 = OutletConnectors.OrderByDescending(x => x.Flow).FirstOrDefault();
            OutletConnector2 = OutletConnectors.OrderByDescending(x => x.Flow).LastOrDefault();
            if (OutletConnector1.Flow == OutletConnector2.Flow)
            {
                InletConnector.Vector = GetVector(InletConnector.Origin, LocPoint);
                OutletConnector1.Vector = GetVector(OutletConnector1.Origin, LocPoint);
                OutletConnector2.Vector = GetVector(OutletConnector2.Origin, LocPoint);
                // After identifying the main connector, set its IsMainConnector property
                StraightTee(InletConnector, OutletConnector1, OutletConnector2);
                CustomConnector swapconnector = null;
                if (OutletConnector2.IsStraight)
                {
                    swapconnector = OutletConnector1;
                    OutletConnector1 = OutletConnector2;
                    OutletConnector2 = swapconnector;
                }
            }

            else
            {
                InletConnector.Vector = GetVector(InletConnector.Origin, LocPoint);
                OutletConnector1.Vector = GetVector(OutletConnector1.Origin, LocPoint);
                OutletConnector2.Vector = GetVector(OutletConnector2.Origin, LocPoint);
                // After identifying the main connector, set its IsMainConnector property
                StraightTee(InletConnector, OutletConnector1, OutletConnector2);
            }
            //Допиши сюда что может быть на отвод
            double relA;
            double relQ;
            double relC;



            TeeData teeData = new TeeData(SystemType, OutletConnector1.IsStraight, InletConnector.Diameter);

            LocRes = teeData.GetLocRes(InletConnector.Diameter);
            if (SystemType == PipeSystemType.SupplyHydronic && OutletConnector1.IsStraight == true)
            {
                Element.DetailType = CustomElement.Detail.TeeStraight;
            }
            else if (SystemType == PipeSystemType.SupplyHydronic && OutletConnector1.IsStraight == false)
            {
                Element.DetailType = CustomElement.Detail.TeeBranch;
            }
            else if (SystemType == PipeSystemType.ReturnHydronic && OutletConnector1.IsStraight == true)
            {
                Element.DetailType = CustomElement.Detail.TeeStraight;
            }
            else if (SystemType == PipeSystemType.ReturnHydronic && OutletConnector1.IsStraight == false)
            {
                Element.DetailType = CustomElement.Detail.TeeMerge;
            }

           
           

            CustomDensity density = new CustomDensity();

            Density = density.GetDensity(Temperature);
            
            PDyn = Density *InletConnector.Velocity * InletConnector.Velocity / 2 * LocRes;


        }

        private XYZ GetVector(XYZ origin, XYZ locPoint)
        {
            XYZ result = null;
            double xorigin = origin.X;
            double yorigin = origin.Y;
            double zorigin = origin.Z;
            double xlocpoint = locPoint.X;
            double ylocpoint = locPoint.Y;
            double zlocpoint = locPoint.Z;

            result = new XYZ(xorigin - xlocpoint, yorigin - ylocpoint, zorigin - zlocpoint);
            return result;

        }
        public void StraightTee(CustomConnector inlet, CustomConnector outlet, CustomConnector outlet2)
        {
            XYZ inletvector = inlet.Vector;
            XYZ outletvector = outlet.Vector;

            double ivX = inletvector.X;
            double ivY = inletvector.Y;
            double ivZ = inletvector.Z;
            double ovX = outletvector.X;
            double ovY = outletvector.Y;
            double ovZ = outletvector.Z;

            double scalar = ivX * ovX + ivY * ovY + ivZ * ovZ;
            double vectorA = Math.Sqrt(Math.Pow(ivX, 2) + Math.Pow(ivY, 2) + Math.Pow(ivZ, 2));
            double vectorB = Math.Sqrt(Math.Pow(ovX, 2) + Math.Pow(ovY, 2) + Math.Pow(ovZ, 2));

            double radian = scalar / (vectorA * vectorB);
            double angle = Math.Round(Math.Acos(radian) * 57.3, 0);
            if (angle == 180)
            {
                outlet.IsStraight = true;
            }
            else
            {
                outlet2.IsStraight = true;
            }

        }
    }
}