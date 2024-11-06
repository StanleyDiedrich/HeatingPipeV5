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

namespace HeatingPipeV5
{

    public class CustomElement
    {
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public ElementId NextElementId { get; set; }
        public MEPSystem MSystem { get; set; }
        public MEPModel Model { get; set; }
        public string SystemName { get; set; }
        public string ShortSystemName { get; set; }
        public string Lvl { get; set; }
        public DuctSystemType SystemType { get; set; }

        public CustomConnector SelectedConnector { get; set; }
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
        public enum Detail
        {
            RectangularDuct,
            RoundDuct,
            Tee,
            Elbow,
            Silencer,
            FireProtectValve,
            AirTerminal,
            Drossel,
            Cap,
            TapAdjustable,

            RoundElbow,
            RectElbow,


            Transition,
            RectTransition,
            RectExpansion,
            RectContraction,
            RoundTransition,


            RoundExpansion,
            RoundContraction,
            RectRoundExpansion,
            RoundRectExpansion,
            RectRoundContraction,
            RoundRectContraction,



            RoundTeeBranch,
            RoundTeeStraight,
            RectTeeBranch,
            RectTeeStraight,
            RectRoundTeeBranch,
            RectRoundTeeStraight,


            RoundFlexDuct,
            RectFlexDuct,

            RoundInRoundDuctInsertStraight,
            RoundInRoundDuctInsertBranch,
            RoundInRectDuctInsertStraight,
            RoundInRectDuctInsertBranch,
            RectInRectDuctInsertStraight,
            RectInRectDuctInsertBranch,
            RectInRoundDuctInsertStraight,
            RectInRoundDuctInsertBranch,

            AirTerminalConnection,
            Union,
            Pipe,
            Valve

        }

        public Detail DetailType { get; set; }
        public int TrackNumber { get; set; }
        public int BranchNumber { get; set; }
        public bool MainTrack { get; set; }
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
            SystemName = Element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();

            if (Element.LookupParameter("Базовый уровень") != null)
            {
                Lvl = Element.LookupParameter("Базовый уровень").AsValueString();
            }

            else
            {
                Lvl = Element.LookupParameter("Уровень").AsValueString();
            }





            if (Element is Duct)
            {
                MSystem = (Element as MEPCurve).MEPSystem;
                SystemType = (MSystem as MechanicalSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();

                OwnConnectors = ((Element as Duct) as MEPCurve).ConnectorManager.Connectors;
                string primaryvolume = Element.get_Parameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM).AsValueString();
                Volume = GetValue(primaryvolume);
                string primarylength = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                ModelLength = primarylength;
                string primaryvelocity = Element.get_Parameter(BuiltInParameter.RBS_VELOCITY).AsValueString();
                ModelVelocity = GetValue(primaryvelocity);
                foreach (Connector connector in OwnConnectors)
                {
                    ConnectorSet nextconnectors = connector.AllRefs;

                    if (connector.Domain != Domain.DomainHvac)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
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

                                if (doc.GetElement(connect.Owner.Id) is MechanicalSystem || doc.GetElement(connect.Owner.Id) is DuctInsulation)
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

                                        if (SystemType == DuctSystemType.SupplyAir)
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;

                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = ((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000).ToString();
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = (Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4).ToString();
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
                                                //SecondaryConnectors.Add(custom);
                                            }

                                        }
                                        else if (SystemType == DuctSystemType.ExhaustAir)
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
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

            if (Element is FlexDuct)
            {
                MSystem = (Element as MEPCurve).MEPSystem;
                SystemType = (MSystem as MechanicalSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();

                OwnConnectors = ((Element as FlexDuct) as MEPCurve).ConnectorManager.Connectors;
                string primaryvolume = Element.get_Parameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM).AsValueString();
                Volume = GetValue(primaryvolume);
                string primarylength = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                ModelLength = primarylength;
                string primaryvelocity = Element.get_Parameter(BuiltInParameter.RBS_VELOCITY).AsValueString();
                ModelVelocity = GetValue(primaryvelocity);

                foreach (Connector connector in OwnConnectors)
                {
                    ConnectorSet nextconnectors = connector.AllRefs;

                    if (connector.Domain != Domain.DomainHvac)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
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

                                if (doc.GetElement(connect.Owner.Id) is MechanicalSystem || doc.GetElement(connect.Owner.Id) is DuctInsulation)
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

                                        if (SystemType == DuctSystemType.SupplyAir)
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;

                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = ((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000).ToString();
                                                    DetailType = Detail.RoundFlexDuct;
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = (Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4).ToString();
                                                    DetailType = Detail.RectFlexDuct;
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
                                                //SecondaryConnectors.Add(custom);
                                            }

                                        }
                                        else if (SystemType == DuctSystemType.ExhaustAir)
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    DetailType = Detail.RoundFlexDuct;
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    DetailType = Detail.RectFlexDuct;
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
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
                else if (Element.Category.Id.IntegerValue == -2008013)
                {
                    DetailType = Detail.AirTerminal;
                }
                /*else if (Element.LookupParameter("ТипДетали").AsString())
                {
                    DetailType = Detail.FireProtectValve;
                }*/

                OwnConnectors = (Element as FamilyInstance).MEPModel.ConnectorManager.Connectors;

                foreach (Connector connector in OwnConnectors)
                {

                    if (connector.Domain != Domain.DomainHvac)
                    {
                        continue;
                    }
                    else
                    {
                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else
                            {
                                SystemType = connect.DuctSystemType;
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

                                if (doc.GetElement(connect.Owner.Id) is MechanicalSystem || doc.GetElement(connect.Owner.Id) is DuctInsulation)
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

                                        if (SystemType == DuctSystemType.SupplyAir)
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
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    ModelDiameter = Math.Round(custom.Diameter * 304.8, 0).ToString();
                                                }
                                                else
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
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter;

                                                //SecondaryConnectors.Add(custom);
                                            }

                                        }
                                        else if (SystemType == DuctSystemType.ExhaustAir)
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
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    ModelDiameter = Math.Round(custom.Diameter * 304.8, 0).ToString();
                                                }
                                                else
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
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter;
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
