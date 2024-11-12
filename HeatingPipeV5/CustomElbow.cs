using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace HeatingPipeV5
{
    public class CustomElbow
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
        public PipeSystemType SystemType { get; set; }
        public MEPSystem MEPSystem { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }

        public double Pressure { get; set; }
        public double LocRes { get; set; }
        public List<CustomConnector> OwnConnectors { get; set; } = new List<CustomConnector>();

        public double PDyn { get; set; }
        public CustomElbow(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element.Element;
            ElementId = element.ElementId;
            ElementId syselementId = null;
            if (document.GetElement(ElementId) is FamilyInstance)
            {
                foreach (Connector connector in element.OwnConnectors)
                {
                    if (connector.Domain != Domain.DomainPiping)
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
                                            custom.Flow = connect.Flow*101.8;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                               
                                                custom.Diameter = connect.Radius * 2 * 304.8;
                                                Diameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter/1000, 2) / 4;
                                                custom.EquiDiameter = custom.Diameter;
                                                try
                                                {
                                                    
                                                    custom.Velocity = custom.Flow / (custom.Area*3600 );
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 6.68);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            OutletConnector = custom;
                                            FlowVelocity = OutletConnector.Velocity;
                                            try
                                            {
                                                syselementId = (connect.MEPSystem as PipingSystem).GetTypeId();
                                                Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);
                                            }
                                            catch
                                            {

                                            }

                                            //SecondaryConnectors.Add(custom);
                                        }


                                    }
                                    else if (SystemType == PipeSystemType.ReturnHydronic)
                                    {
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow*101.8;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {

                                                custom.Diameter = connect.Radius * 2 * 304.8;
                                                Diameter = custom.Diameter;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter/1000, 2) / 4;
                                                try
                                                {
                                                   
                                                    custom.Velocity = custom.Flow / (custom.Area*3600);
                                                }
                                                catch { }

                                            }
                                            else
                                            {
                                                
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 6.68);
                                            }
                                            custom.Coefficient = connect.Coefficient;

                                            OutletConnector = custom;
                                            FlowVelocity = OutletConnector.Velocity;
                                            try
                                            {
                                                syselementId = (connect.MEPSystem as PipingSystem).GetTypeId();
                                                Temperature = Convert.ToDouble(Document.GetElement(syselementId).get_Parameter(BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM).AsValueString().Split()[0]);
                                            }
                                            catch
                                            {

                                            }
                                           
                                            //SecondaryConnectors.Add(custom);
                                        }
                                    }

                                }

                            }

                        }
                    }

                }
            }

            

           
            ElbowData elbowdata = new ElbowData();
           
            CustomDensity density = new CustomDensity();
            Density=density.GetDensity(Temperature);
            LocRes = elbowdata.GetLocRes(Diameter);
            PDyn = Density * FlowVelocity * FlowVelocity / 2*LocRes; 


        }
    }
}

