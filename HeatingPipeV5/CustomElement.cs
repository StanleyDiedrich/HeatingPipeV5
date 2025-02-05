using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System.Runtime.Remoting.Contexts;

using Autodesk.Revit.Creation;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB.Plumbing;
using System.Windows.Documents;

namespace HeatingPipeV5
{

    public class CustomElement
    {
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public string ElementName { get; set; }
        public ElementId NextElementId { get; set; }
        public MEPSystem MSystem { get; set; }
        public MEPModel Model { get; set; }
        public string SystemName { get; set; }
        public string ShortSystemName { get; set; }
        public string Lvl { get; set; }
        public PipeSystemType SystemType { get; set; }
       
        public CustomConnector SelectedConnector { get; set; }
        public CustomConnector SupplyConnector { get; set; }
        public CustomConnector ReturnConnector { get; set; }
        public List<CustomConnector> SecondaryConnectors { get; set; }

        public ConnectorSet OwnConnectors { get; set; }
        public string Volume { get; set; }
        public string ModelWidth { get; set; }
        public string ModelHeight { get; set; }
        public string ModelLength { get; set; }
        public string ModelDiameter { get; set; }
        public string ModelVelocity { get; set; }
        public string ModelHydraulicDiameter { get; set; }
        public double EquiDiameter { get; set; }
        public string ModelHydraulicArea { get; set; }
        public double LocRes { get; set; }
        public double PDyn { get; set; }
        public double PStat { get; set; }
        public double Ptot { get; set; }
        public double Ltot { get; set; }
        public double Lenght { get; set; }
        public enum Detail
        {
            
            Tee,
            Elbow,
            Silencer,
            FireProtectValve,
            Equipment,
            Drossel,
            Cap,
            TapAdjustable,
            Transition,
            RoundTransition,
            RoundFlexDuct,
            RectFlexDuct,
            AirTerminalConnection,
            Union,
            Pipe,
            FlexPipe,
            Valve,
            Manifold,
            TeeStraight,
            TeeBranch,
            TeeMerge,
            Expansion,
            Contraction
            

        }

        public Detail DetailType { get; set; }
        public int TrackNumber { get; set; }
        public int BranchNumber { get; set; }
        public int GroupNumber { get; set; }
        public int LevelNumber { get; set; }
        public bool MainTrack { get; set; }
        public string RelPres { get; set; }
        private string GetValue(string primaryvolume)
        {
            // Используем регулярное выражение, чтобы найти и вернуть только числовую часть
            var match = System.Text.RegularExpressions.Regex.Match(primaryvolume, @"\d+(\.\d+)?");
            return match.Success ? match.Value : string.Empty; // Вернуть число или пустую строку, если числ
        }
        public CustomElement(Autodesk.Revit.DB.Document doc, ElementId elementId)
        {
            if (elementId == null)
            {
                return;
            }
            ElementId = elementId;
            Element = doc.GetElement(ElementId);
            ElementName = Element.Name;
            SystemName = Element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();

            if (Element.LookupParameter("Базовый уровень") != null)
            {
                Lvl = Element.LookupParameter("Базовый уровень").AsValueString();
            }

            else
            {
                Lvl = Element.LookupParameter("Уровень").AsValueString();
            }

            if (elementId.IntegerValue == 2360288)
            {
                var nextelement2 = elementId;
            }



            if (Element is Pipe)
            {
                MSystem = (Element as MEPCurve).MEPSystem;
                SystemType = (MSystem as PipingSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();

                OwnConnectors = ((Element as Pipe) as MEPCurve).ConnectorManager.Connectors;
                string primaryvolume = Element.get_Parameter(BuiltInParameter.RBS_PIPE_FLOW_PARAM).AsValueString();
                Volume = GetValue(primaryvolume);
                string primarylength = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                ModelLength = primarylength;
                string primaryvelocity = Element.get_Parameter(BuiltInParameter.RBS_PIPE_VELOCITY_PARAM).AsValueString();
                ModelVelocity = GetValue(primaryvelocity);
                foreach (Connector connector in OwnConnectors)
                {
                    ConnectorSet nextconnectors = connector.AllRefs;

                    if (connector.Domain == Domain.DomainUndefined)
                    {
                        continue;
                    }
                    else if (connector.Domain == Domain.DomainPiping || connector.Domain == Domain.DomainHvac)
                    {
                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain == Domain.DomainUndefined)
                            {
                                continue;
                            }
                            else
                            {
                                CustomConnector custom = new CustomConnector(doc, ElementId, SystemType);
                                try
                                {
                                    ShortSystemName = doc.GetElement(connect.Owner.Id).get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();
                                }
                                catch
                                {

                                }
                                if (ShortSystemName == null || ShortSystemName == string.Empty)
                                {
                                    continue;
                                }

                                if (doc.GetElement(connect.Owner.Id) is PipingSystem || doc.GetElement(connect.Owner.Id) is PipeInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }

                                else if (ShortSystemName.Contains(ShortSystemName))
                                {
                                    if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                    {

                                        if (SystemType == PipeSystemType.SupplyHydronic)
                                        {

                                            if (connect.Direction == FlowDirectionType.Out)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                custom.DirectionType = FlowDirectionType.Out;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    DetailType = Detail.Pipe;
                                                    custom.Diameter = connect.Radius * 2;
                                                    //custom.EquiDiameter = custom.Diameter;
                                                    //string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    // ModelDiameter = primarydiameter;

                                                    /*ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = ((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000).ToString();*/
                                                }
                                               
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                
                                                //SecondaryConnectors.Add(custom);
                                            }

                                        }
                                        else if (SystemType == PipeSystemType.ReturnHydronic)
                                        {
                                            if (connect.Direction == FlowDirectionType.In)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                custom.DirectionType = FlowDirectionType.In;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    DetailType = Detail.Pipe;
                                                    custom.Diameter = connect.Radius * 2;
                                                    //custom.EquiDiameter = custom.Diameter;
                                                    //string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    //ModelDiameter = primarydiameter;
                                                   
                                                }
                                                
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                
                                            }
                                        }

                                    }
                                }
                            }

                        }




                    }
                }



            }

            if (Element is FlexPipe)
            {
                MSystem = (Element as MEPCurve).MEPSystem;
                SystemType = (MSystem as PipingSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();

                OwnConnectors = ((Element as FlexPipe) as MEPCurve).ConnectorManager.Connectors;
                string primaryvolume = Element.get_Parameter(BuiltInParameter.RBS_PIPE_FLOW_PARAM).AsValueString();
                Volume = GetValue(primaryvolume);
                string primarylength = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                ModelLength = primarylength;
                string primaryvelocity = Element.get_Parameter(BuiltInParameter.RBS_PIPE_VELOCITY_PARAM).AsValueString();
                ModelVelocity = GetValue(primaryvelocity);

                foreach (Connector connector in OwnConnectors)
                {
                    ConnectorSet nextconnectors = connector.AllRefs;

                    if (connector.Domain == Domain.DomainUndefined)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (Connector connect in nextconnectors)
                        {
                            if (connector.Domain != Domain.DomainPiping || connector.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else if  (connector.Domain == Domain.DomainPiping || connector.Domain == Domain.DomainHvac)
                            {
                                CustomConnector custom = new CustomConnector(doc, ElementId, SystemType);
                                try
                                {
                                    ShortSystemName = doc.GetElement(connect.Owner.Id).get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();
                                }
                                catch
                                {

                                }
                                if (ShortSystemName == null || ShortSystemName == string.Empty)
                                {
                                    continue;
                                }

                                if (doc.GetElement(connect.Owner.Id) is PipingSystem || doc.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }

                                else if (ShortSystemName.Contains(ShortSystemName))
                                {
                                    if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                    {

                                        if (SystemType == PipeSystemType.SupplyHydronic)
                                        {

                                            if (connect.Direction == FlowDirectionType.Out)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                custom.DirectionType = FlowDirectionType.Out;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    DetailType = Detail.FlexPipe;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;

                                                    
                                                }
                                                
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
                                               
                                            }

                                        }
                                        else if (SystemType == PipeSystemType.ReturnHydronic)
                                        {
                                            if (connect.Direction == FlowDirectionType.In)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                custom.DirectionType = FlowDirectionType.In;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    DetailType = Detail.FlexPipe;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;
                                                    
                                                }
                                                
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
                                                
                                            }
                                        }

                                    }
                                }
                            }

                        }




                    }
                }



            }






            if (Element is FamilyInstance)
            {
                Model = (Element as FamilyInstance).MEPModel;
                if ((Model as MechanicalFitting) != null)
                {
                    if ((Model as MechanicalFitting).PartType == PartType.Cap)
                    {
                        DetailType = Detail.Cap;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Elbow)
                    {
                        DetailType = Detail.Elbow;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Tee)
                    {
                        DetailType = Detail.Tee;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.TapAdjustable)
                    {
                        DetailType = Detail.TapAdjustable;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Transition)
                    {
                        DetailType = Detail.Transition;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Union)
                    {
                        DetailType = Detail.Union;
                    }


                }
                else if (Element.Category.Id.IntegerValue == -2008016)
                {
                    DetailType = Detail.FireProtectValve;
                }
                else if (Element.Category.Id.IntegerValue == -2001140)
                {
                    DetailType = Detail.Equipment;
                }
                /*else if (Element.LookupParameter("ТипДетали").AsString())
                {
                    DetailType = Detail.FireProtectValve;
                }*/



                OwnConnectors = (Element as FamilyInstance).MEPModel.ConnectorManager.Connectors;

                foreach (Connector connector in OwnConnectors)
                {

                    if (connector.Domain == Domain.DomainUndefined )
                    {
                        continue;
                    }
                    else
                    {
                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            
                             if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                            {
                                SystemType = connect.PipeSystemType;
                                CustomConnector custom = new CustomConnector(doc, ElementId, SystemType);
                                try
                                {
                                    ShortSystemName = doc.GetElement(connect.Owner.Id).get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();
                                }
                                catch
                                {

                                }
                                if (ShortSystemName == null || ShortSystemName == string.Empty)
                                {
                                    continue;
                                }

                                if (doc.GetElement(connect.Owner.Id) is PipingSystem || doc.GetElement(connect.Owner.Id) is PipeInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }

                                else if (ShortSystemName.Contains(ShortSystemName))
                                {
                                    if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                    {

                                        if (SystemType == PipeSystemType.SupplyHydronic)
                                        {

                                            if (connect.Direction == FlowDirectionType.Out || connect.Direction == FlowDirectionType.Bidirectional)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                //custom.DirectionType = FlowDirectionType.Out;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    ModelDiameter = Math.Round(custom.Diameter * 304.8, 0).ToString();
                                                    
                                                }
                                               /* else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    try
                                                    {
                                                        ModelWidth = doc.GetElement(ElementId).LookupParameter("Ширина воздуховода").AsValueString();
                                                        ModelHeight = doc.GetElement(ElementId).LookupParameter("Высота воздуховода").AsValueString();
                                                        double mwidth = Convert.ToDouble(ModelWidth);
                                                        double mheight = Convert.ToDouble(ModelHeight);
                                                        ModelHydraulicDiameter = Convert.ToInt32(2 * mwidth * mheight / (mwidth + mheight)).ToString();
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }*/
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter;
                                                SupplyConnector = custom;
                                                //SecondaryConnectors.Add(custom);
                                            }

                                        }
                                        else if (SystemType == PipeSystemType.ReturnHydronic)
                                        {
                                            if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                //custom.DirectionType = FlowDirectionType.In;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    ModelDiameter = Math.Round(custom.Diameter * 304.8, 0).ToString();
                                                }
                                                /*else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    try
                                                    {
                                                        ModelWidth = doc.GetElement(ElementId).LookupParameter("Ширина воздуховода").AsValueString();
                                                        ModelHeight = doc.GetElement(ElementId).LookupParameter("Высота воздуховода").AsValueString();
                                                        double mwidth = Convert.ToDouble(ModelWidth);
                                                        double mheight = Convert.ToDouble(ModelHeight);
                                                        ModelHydraulicDiameter = Convert.ToInt32(2 * mwidth * mheight / (mwidth + mheight)).ToString();
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }*/
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter;
                                                ReturnConnector = custom;
                                                //SecondaryConnectors.Add(custom);
                                            }
                                        }

                                    }
                                }
                            }

                        }



                    }
                }

            }


        }


    }
}
