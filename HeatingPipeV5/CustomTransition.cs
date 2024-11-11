using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System.Windows.Media.Animation;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.ExtensibleStorage;


namespace HeatingPipeV5
{
    public class CustomTransition
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double Diameter { get; set; }
        public double ElbowRadius { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public PipeSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public double RelA { get; set; }
        public double Angle { get; set; }

        public double Velocity { get; set; }
        public double Density { get; set; }
        public double Temperature { get; set; }
        public double PDyn { get; set; }
        public CustomTransition(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;

            if (element.Element is Pipe)
            {
                element.DetailType = CustomElement.Detail.Pipe;
                CustomPipe customPipe = new CustomPipe(Document, element);
                element.Ptot = customPipe.Pressure;
                return;
            }

            if (document.GetElement(ElementId) is FamilyInstance)
            {
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
                                                try
                                                {
                                                    ElementId syselementId = (connect.MEPSystem as PipingSystem).GetTypeId();
                                                    Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);

                                                }
                                                catch
                                                { }

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
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;

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

                                                try
                                                {
                                                    ElementId syselementId = (connect.MEPSystem as PipingSystem).GetTypeId();
                                                    Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);

                                                }
                                                catch
                                                { }

                                            }
                                          /*  else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
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

                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
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
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                        }
                                    }

                                }

                            }

                        }
                    }

                }

                CustomDensity density = new CustomDensity();

                Density = density.GetDensity(Temperature);

                

                if (InletConnector.AInlet > OutletConnector.AOutlet)
                {
                    Element.DetailType = CustomElement.Detail.Expansion;
                    Element.LocRes = 1;
                    PDyn = Density * InletConnector.Velocity * InletConnector.Velocity / 2 * LocRes;
                }
                else if (InletConnector.AInlet < OutletConnector.AOutlet)
                {
                    Element.DetailType = CustomElement.Detail.Contraction;
                    Element.LocRes = 0.5;
                    PDyn = Density * InletConnector.Velocity * InletConnector.Velocity / 2 * LocRes;
                }
                else if (InletConnector.AInlet == OutletConnector.AOutlet)
                {
                    Element.DetailType = CustomElement.Detail.Union;
                    Element.LocRes = 0;
                }

            }




        }
    }
}
