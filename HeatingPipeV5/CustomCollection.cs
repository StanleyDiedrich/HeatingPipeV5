using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Globalization;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Windows.Input;
using System.Xml.Linq;

namespace HeatingPipeV5
{


    public class CustomCollection
    {
        List<CustomBranch> Collection { get; set; } = new List<CustomBranch>();
        Autodesk.Revit.DB.Document Document { get; set; }
        public double Density { get; set; }

        public void Add(CustomBranch branch)
        {
            Collection.Add(branch);
        }

        public void CreateBranch(Document document, ElementId airterminal)
        {
            CustomBranch customBranch = new CustomBranch(Document, airterminal);
            try
            {
                customBranch.CreateNewBranch(Document, airterminal);
                CustomElement customElement = new CustomElement(Document, airterminal);
                Collection.Add(customBranch);
            }
            catch
            { }


        }
        public CustomCollection(Autodesk.Revit.DB.Document doc)
        {
            Document = doc;

        }

        public CustomCollection(Autodesk.Revit.DB.Document doc, List<CustomBranch> branches)
        {
            Document = doc;
            Collection = branches;
        }

        public List<ElementId> ShowElements(int number)
        {
            // Параметр number должен находиться в допустимом диапазоне
            if (number < 0 || number >= Collection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Значение number должно быть в пределах диапазона коллекции.");
            }

            List<ElementId> elements = new List<ElementId>();

            // Перебираем все ветви в указанной коллекции



            // Перебираем все элементы в текущей ветви
            foreach (var element in Collection[number].Elements)
            {
                if (element != null) // проверяем, что элемент не null
                {
                    elements.Add(element.ElementId);
                }
            }


            return elements;
        }


        public void Calcualate(double density)
        {
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "," };
            IFormatProvider formatter2 = new NumberFormatInfo { NumberDecimalSeparator = "." };
            Density = density;
            foreach (var branch in Collection)
            {
                if (branch.Elements.Count == 0 || branch == null)
                { continue; }
                try
                {
                    foreach (var element in branch.Elements)
                    {
                      if (element.ElementId.IntegerValue==4080858)
                        {
                            var element3 = element;
                        }

                        if (element.ElementId.IntegerValue == 4080858)
                        {
                            var element2 = element;
                        }

                        if (element.DetailType == CustomElement.Detail.Equipment)
                        {
                            if (element.ElementId.IntegerValue == 5807241)
                            {
                                var element2 = element;
                            }

                            if (element.Element.LookupParameter("ADSK_Расход теплоносителя") != null)
                            {
                                element.Volume = element.Element.LookupParameter("ADSK_Расход теплоносителя").AsValueString().Split()[0];
                            }
                            if (element.Element.LookupParameter("Расход воды") != null)
                            {
                                element.Volume = element.Element.LookupParameter("Расход воды").AsValueString().Split()[0];
                            }
                            try
                            {
                                element.RoomName = (element.Element as FamilyInstance).Space.Name;
                            }
                            catch
                            {

                            }
                            try
                            {
                                element.TempIn = (element.Element as FamilyInstance).LookupParameter("ADSK_Температура подающей линии").AsValueString();
                                element.TempOut = (element.Element as FamilyInstance).LookupParameter("ADSK_Температура обратной линии").AsValueString();
                                element.TempRegime = $"{element.TempIn}/{element.TempOut}";
                            }
                            catch
                            {

                            }
                            try
                            {
                                element.HeatLoss = (element.Element as FamilyInstance).LookupParameter("ADSK_Теплопотери").AsValueString();
                            }
                            catch
                            {

                            }
                            branch.Pressure += 8000;
                            branch.Length += 0;

                        }
                        else if (element.OwnConnectors.Size > 3)
                        {
                            element.DetailType = CustomElement.Detail.Manifold;
                            element.LocRes = 1.1;
                            branch.Pressure += 5000;
                            element.Unit = "шт";
                        }
                        else if (element.DetailType == CustomElement.Detail.Elbow)
                        {
                            if (element.ElementId.IntegerValue == 2794589)
                            {
                                var element2 = element;
                            }
                            CustomElbow customElbow = new CustomElbow(Document, element);
                            element.LocRes = customElbow.LocRes;
                            element.PDyn = customElbow.PDyn;
                            element.ModelLength = "1";
                            element.DiameterOuter = element.Element.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
                            //CustomElbow customElbow = new CustomElbow(Document, element);
                            //element.LocRes = customElbow.LocRes;
                            //element.PDyn = Density * Math.Pow(customElbow.Velocity, 2) / 2 * element.LocRes;
                            branch.Pressure += element.PDyn;
                            element.Unit = "шт";
                        }
                        else if (element.DetailType == CustomElement.Detail.Tee)
                        {
                            if (element.ElementId.IntegerValue == 4083974)
                            {
                                var element2 = element;
                            }
                            if (element.Element is Pipe)
                            {
                                element.DetailType = CustomElement.Detail.Pipe;
                                CustomPipe customPipe = new CustomPipe(Document, element);
                                element.Volume = customPipe.Volume;
                                element.ModelVelocity = Convert.ToString(customPipe.FlowVelocity);
                                element.ModelVelocity = Convert.ToString(customPipe.FlowVelocity);
                                element.ModelDiameter = Convert.ToString(customPipe.Diameter * 1000);
                                element.ModelLength = Convert.ToString(Math.Round(customPipe.Length, 2));

                                element.PStat = Math.Round(customPipe.Pressure, 2);
                                element.RelPres = Convert.ToString(Math.Round(element.PStat / Math.Round(customPipe.Length, 2), 2));
                                branch.Pressure += customPipe.Pressure;
                                element.Lenght += customPipe.Length;
                                branch.Length += customPipe.Length;
                                element.Unit = "шт";
                            }
                            else
                            {
                               // CustomTee customTee = new CustomTee(Document, element);
                                //element.LocRes = customTee.LocRes;
                               // element.PDyn = customTee.PDyn;
                                element.ModelLength = "-";
                                //element.Volume = String.Join("-", Math.Round(customTee.InletConnector.Flow, 2),
                                 //            Math.Round(customTee.OutletConnector1.Flow, 2),
                                   //          Math.Round(customTee.OutletConnector2.Flow, 2));
                                //element.ModelVelocity = String.Join("-", Math.Round(customTee.InletConnector.Velocity, 2), Math.Round(customTee.OutletConnector1.Velocity, 2), Math.Round(customTee.OutletConnector2.Velocity, 2));
                                //CustomTee customTee = new CustomTee(Document, element);
                                //element.LocRes = customTee.LocRes;
                                //element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
                                branch.Pressure += element.PDyn;
                               
                                element.Unit = "шт";
                            }

                        }
                        /*else if (element.DetailType == CustomElement.Detail.TapAdjustable)
                        {

                            if (element.ElementId.IntegerValue == 6246776)

                            {
                                var element2 = element;
                            }

                            CustomDuctInsert customDuctInsert = new CustomDuctInsert(Document, element);
                            element.LocRes = customDuctInsert.LocRes;
                            element.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes;
                            branch.Pressure += 1;
                        }*/
                        else if (element.DetailType == CustomElement.Detail.Transition)
                        {
                           if (element.ElementId.IntegerValue == 4084072)
                            {
                                var el = element;
                            }

                            CustomTransition customTransition = new CustomTransition(Document, element);
                            element.LocRes = customTransition.LocRes;
                            element.PDyn = customTransition.PDyn;
                            element.ModelLength = "1";
                            element.Unit = "шт";
                            element.DiameterOuter = element.Element.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString().Split('-')[0];
                            //CustomTransition customTransition = new CustomTransition(Document, element);

                            /*element.LocRes = customTransition.LocRes;
                            element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;*/

                            branch.Pressure += element.PDyn;
                        }

                        else if (element.DetailType == CustomElement.Detail.Pipe)
                        {
                            if (element.ElementId.IntegerValue == 2214675)
                            {
                                ElementId el = element.ElementId;
                            }
                            CustomPipe customPipe = new CustomPipe(Document, element);
                            element.Volume = customPipe.Volume;
                            element.ModelVelocity = Convert.ToString(customPipe.FlowVelocity);
                            element.ModelVelocity = Convert.ToString(customPipe.FlowVelocity);
                            element.ModelDiameter = Convert.ToString(customPipe.Diameter * 1000);
                            element.ModelLength = Convert.ToString(Math.Round(customPipe.Length, 2));

                            element.PStat = Math.Round(customPipe.Pressure, 2);
                            element.RelPres = Convert.ToString(Math.Round(element.PStat / Math.Round(customPipe.Length, 2), 2));
                            branch.Pressure += customPipe.Pressure;
                            element.Lenght += customPipe.Length;
                            branch.Length += customPipe.Length;
                            element.Unit = "м";
                            element. DiameterInner = (Convert.ToDouble(element.Element.get_Parameter(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM).AsValueString())).ToString();
                            element.DiameterOuter = (Convert.ToDouble(element.Element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsValueString())).ToString();
                            /*branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PIPE_PRESSUREDROP_PARAM).AsDouble();
                            string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PIPE_PRESSUREDROP_PARAM).AsValueString().Split();
                            element.PStat = double.Parse(pressureDropString[0], formatter);*/
                        }
                        else if (element.DetailType == CustomElement.Detail.Valve)
                        {
                            branch.Pressure += 10000;
                            element.Unit = "шт";
                        }
                        else if (element.DetailType == CustomElement.Detail.Union)
                        {
                            element.ModelDiameter = "-";
                            element.ModelLength = "-";
                            branch.Pressure += 0;
                            element.Unit = "шт";
                        }
                        branch.RelPressure = branch.Pressure / branch.Length;
                    }
                }
                catch
                {

                }
            }
        }
        public void ResCalculate()
        {
            foreach (var branch in Collection)
            {
                branch.PBTot = 0;
                branch.LTot = 0;
                for (int i = 1; i < branch.Elements.Count; i++)
                {

                    branch.Elements[i].Ptot = Math.Round(branch.Elements[i].PDyn + branch.Elements[i].PStat + branch.Elements[i - 1].Ptot, 2);
                    branch.Elements[i].Ltot = Math.Round(branch.Elements[i].Lenght, 2) + branch.Elements[i - 1].Ltot;

                }

            }
        }
        public void MarkBranches()
        {
            foreach (var branch in Collection)
            {
                
                foreach (var element in branch.Elements)
                {
                    element.BranchNumber = branch.Number;
                    element.TrackNumber++;
                }
            }
            
        }
        public CustomBranch SelectMainBranch()
        {
            List<CustomBranch> branches = new List<CustomBranch>();
            foreach (var branch in Collection)
            {
                branches.Add(branch);
            }
            //var maxbranch = branches.OrderByDescending(x => x.RelPressure).FirstOrDefault(); // добавил так как ОЦК должен быть по максимальному R/l
            var maxbranch = branches.OrderByDescending(x => x.Pressure).FirstOrDefault();
            return maxbranch;
        }


        public void MarkCollection(CustomBranch customBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();

            foreach (var branch in Collection)
            {
                var controlelement = branch.Elements.First().ElementId;
                
                CustomElement previouselement = null;
                int groupnumber2 = 1;
                if (branch.Number == customBranch.Number)
                {

                    int trackCounter = 0;
                    int levelCounter = 1;
                    for (int i = 0; i < branch.Elements.Count; i++)
                    {
                       if (i !=0 )
                        {
                            CustomElement prevelement = branch.Elements[i - 1];
                            CustomElement tempelement = branch.Elements[i];

                            if (prevelement.IsSupply==true && tempelement.IsSupply==false)
                            {
                                trackCounter = 0;
                                levelCounter = 1;
                            }

                        }


                       
                        //var previouselement = branch.Elements[i-1];
                        var element = branch.Elements[i];
                        element.MainTrack = true;
                        if (element.LevelNumber == 0)
                        {
                            element.LevelNumber = levelCounter;
                        }

                        if (element.ElementId.IntegerValue == 4992064)
                        {
                            var element2 = element;
                        }

                        if (previouselement != null)
                        {
                            if (previouselement.DetailType == CustomElement.Detail.Pipe && element.DetailType == CustomElement.Detail.Pipe)
                            {
                                if (previouselement.Volume == element.Volume)
                                {
                                    double d1 = Convert.ToDouble(previouselement.ModelDiameter);
                                    double d2 = Convert.ToDouble(element.ModelDiameter);
                                    if (d1 == d2)
                                    {
                                        element.TrackNumber = trackCounter;
                                        element.BranchNumber = branch.Number;
                                        element.LevelNumber = levelCounter;
                                        //branch.Add(element);
                                        checkedElements.Add(element.ElementId);
                                    }
                                }
                                else
                                {
                                    element.TrackNumber = trackCounter;
                                    element.BranchNumber = branch.Number;
                                    element.LevelNumber = levelCounter;
                                    //branch.Add(element);
                                    checkedElements.Add(element.ElementId);
                                    trackCounter++;

                                }
                            }
                            else if (element.DetailType == CustomElement.Detail.Manifold)
                            {
                                trackCounter = 0;
                                element.TrackNumber = 0;
                                element.BranchNumber = branch.Number;
                                levelCounter *= 10;
                                element.LevelNumber *= levelCounter;
                                //branch.Add(element);
                                checkedElements.Add(element.ElementId);

                                //trackCounter++;
                            }
                            else
                            {
                                element.TrackNumber = trackCounter;
                                element.BranchNumber = branch.Number;
                                element.LevelNumber = levelCounter;
                                //branch.Add(element);
                                checkedElements.Add(element.ElementId);
                            }
                        }


                        if (element.DetailType == CustomElement.Detail.Pipe)
                        {
                            element.TrackNumber = trackCounter;
                            element.BranchNumber = branch.Number;
                            element.LevelNumber = levelCounter;
                            //branch.Add(element);
                            checkedElements.Add(element.ElementId);
                            previouselement = element;
                        }
                        else
                        {
                            element.TrackNumber = trackCounter;
                            element.BranchNumber = branch.Number;
                            element.LevelNumber = levelCounter;
                            //branch.Add(element);
                            checkedElements.Add(element.ElementId);
                        }

                    }

                    newCustomCollection.Add(branch);
                    break;
                }
            }


            
                CustomElement previouselement2 = null;
                foreach (var branch in Collection)
                {
                    if (branch.Number == customBranch.Number)
                    {
                        continue;
                    }

                    else
                    {
                        int trackCounter = 0;
                        int levelCounter = 1;
                        for (int i = 0; i < branch.Elements.Count; i++)
                        {
                            if (i != 0)
                            {
                                CustomElement prevelement = branch.Elements[i - 1];
                                CustomElement tempelement = branch.Elements[i];

                                if (prevelement.IsSupply == true && tempelement.IsSupply == false)
                                {
                                    trackCounter = 0;
                                    levelCounter = 1;
                                }

                            }
                        //var previouselement = branch.Elements[i-1];
                        var element = branch.Elements[i];
                            if (checkedElements.Contains(element.ElementId))
                            {
                                continue;
                            }
                            else
                            {
                                if (element.LevelNumber == 0)
                                {
                                    element.LevelNumber = levelCounter;
                                }

                                if (element.ElementId.IntegerValue == 4241799)
                                {
                                    var element2 = element;
                                }
                                if (previouselement2 != null)
                                {
                                    if (previouselement2.DetailType == CustomElement.Detail.Pipe && element.DetailType == CustomElement.Detail.Pipe)
                                    {
                                        if (previouselement2.Volume == element.Volume)
                                        {
                                            double d1 = Convert.ToDouble(previouselement2.ModelDiameter);
                                            double d2 = Convert.ToDouble(element.ModelDiameter);
                                            if (d1 == d2)
                                            {
                                                element.TrackNumber = trackCounter;
                                                element.BranchNumber = branch.Number;
                                                element.LevelNumber = levelCounter;

                                                //branch.Add(element);
                                                checkedElements.Add(element.ElementId);
                                            }
                                        }
                                        else
                                        {
                                            element.TrackNumber = trackCounter;
                                            element.BranchNumber = branch.Number;
                                            element.LevelNumber = levelCounter;

                                            //branch.Add(element);
                                            checkedElements.Add(element.ElementId);
                                            trackCounter++;

                                        }
                                    }
                                    else if (element.DetailType == CustomElement.Detail.Manifold)
                                    {
                                        trackCounter = 0;
                                        element.TrackNumber = 0;
                                        element.BranchNumber = branch.Number;

                                        levelCounter *= 10;
                                        element.LevelNumber *= levelCounter;
                                        //branch.Add(element);
                                        checkedElements.Add(element.ElementId);

                                        //trackCounter++;
                                    }
                                    else
                                    {
                                        element.TrackNumber = trackCounter;
                                        element.BranchNumber = branch.Number;
                                        element.LevelNumber = levelCounter;

                                        //branch.Add(element);
                                        checkedElements.Add(element.ElementId);
                                    }
                                }


                                if (element.DetailType == CustomElement.Detail.Pipe)
                                {
                                    previouselement2 = element;
                                }

                            }

                        }
                    }

                    newCustomCollection.Add(branch);
                }

            var uniqueElements = new HashSet<ElementId>();

            foreach (var branch in newCustomCollection)
            {
                // Создание новой коллекции для хранения уникальных элементов
                var uniqueBranchElements = new List<CustomElement>();

                foreach (var element in branch.Elements)
                {
                    // Проверка на уникальность ElementId
                    if (uniqueElements.Add(element.ElementId))
                    {
                        uniqueBranchElements.Add(element);
                    }
                }

                // Обновление ветки, оставляя только уникальные элементы
                branch.Elements = uniqueBranchElements;
            }
            Collection = newCustomCollection;
        }

        private double ConvertVolume(string volume)
        {
            double result = 0;
            if (string.IsNullOrWhiteSpace(volume))
            {
                //Console.WriteLine("Строка пуста");
                result = 0;
            }

            // Попытка преобразовать строку в число
            if (double.TryParse(volume, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            return result;
        }
























        public string PrepareContent()
        {
            var csvcontent = new StringBuilder();
            csvcontent.AppendLine("Level Архитектурный;DetailType;BranchNumber;ManifoldNumber;SectionNumber;Dв;Dн;Length;Unit;ElementIds;Code;Name;LevelAudithor;Space;Adsk_Теплопотери;График");

            foreach (var branch in Collection)
            {
                var elements = branch.Elements
                    .GroupBy(s => new { s.Lvl, s.SystemName, s.DetailType, s.LevelNumber, s.TrackNumber })
                    .SelectMany(g => g.Select(element => new
                    {
                        element.Lvl,
                        element.SystemName,
                        element.BranchNumber,
                        element.DetailType,
                        element.LevelNumber,
                        element.TrackNumber,
                        element.DiameterInner,
                        element.DiameterOuter,
                        element.ModelLength,
                        element.Unit,
                        element.ElementId,
                        element.ShortSystemName,
                        element.ElementName,
                        element.AuditorLevel,
                        element.RoomName,
                        element.HeatLoss,
                        element.TempRegime
                    }));

                foreach (var element in elements)
                {
                    string a = $"{element.Lvl};{element.DetailType};{element.BranchNumber};{element.LevelNumber};{element.TrackNumber};{element.DiameterInner};{element.DiameterOuter};" +
                                $"{element.ModelLength};{element.Unit};{element.ElementId};{element.ShortSystemName}-{element.Lvl}-{element.BranchNumber}-{element.LevelNumber}-{element.TrackNumber};{element.ElementName};{element.AuditorLevel};{element.RoomName};{element.HeatLoss};{element.TempRegime};";
                    csvcontent.AppendLine(a);
                }
            }
            return csvcontent.ToString();
        }






        public string GetContent()
        {
            var csvcontent = new StringBuilder();
            csvcontent.AppendLine("Id;Level Архитектурный;DetalType;BranchNumber;ManifoldNumber;SectionNumber;Dв;Dн;Length;Unit;ElementIds ;Code;Name;LevelAudithor;Space;Adsk_Теплопотери;График"); ;

           

            foreach (var branch in Collection)
            {
                
                foreach (var element in branch.Elements)
                {
                    string a = $"{element.ElementId};{element.Lvl};{element.DetailType};{element.BranchMark};{element.LevelNumber};{element.TrackNumber};{element.DiameterInner};{element.DiameterOuter};" +
                         $"{element.ModelLength};{element.Unit};{element.ElementId};{element.ShortSystemName}-{element.Lvl}-{element.BranchMark}-{element.LevelNumber}-{element.TrackNumber};{element.ElementName};{element.AuditorLevel};{element.RoomName};{element.HeatLoss};{element.TempRegime};";
                        
                    csvcontent.AppendLine(a);
                }
               
            }

            return csvcontent.ToString();
        }









       /* public string GetContent()
        {
            var csvcontent = new StringBuilder();
            csvcontent.AppendLine("ElementId;DetailType;Name;SystemName;Level;LevelNumber;BranchNumber;SectionNumber;Volume;Length;Diameter;Velocity;PStat;RelPress;KMS;PDyn;Ltot;Ptot;Code;MainTrack");

            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    string a = $"{element.ElementId};{element.DetailType};{element.ElementName};{element.ShortSystemName};{element.Lvl};{element.LevelNumber};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelDiameter};{element.ModelVelocity};{element.PStat};{element.RelPres};{element.LocRes};{element.PDyn};{element.Ltot};{element.Ptot};" +
                         $"{element.ShortSystemName}-{element.Lvl}-{element.BranchNumber}-{element.LevelNumber}-{element.TrackNumber};{element.MainTrack}";
                    csvcontent.AppendLine(a);
                }
            }

            return csvcontent.ToString();
        }*/
        public void SaveFile(string content) // спрятали функцию сохранения 
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.Title = "Save CSV File";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.Write(content);
                    }

                    Console.WriteLine("CSV file saved successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error saving CSV file: " + ex.Message);
                }
            }
        }


        public List<ElementId> ShowElements()
        {
            List<ElementId> selectedelements = new List<ElementId>();
            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (!selectedelements.Contains(element.ElementId))
                    {
                        selectedelements.Add(element.ElementId);
                    }
                }
            }
            return selectedelements;
        }
        private string GetValue(string primaryvolume)
        {
            // Используем регулярное выражение, чтобы найти и вернуть только числовую часть
            var match = System.Text.RegularExpressions.Regex.Match(primaryvolume, @"\d+(\.\d+)?");
            return match.Success ? match.Value : string.Empty; // Вернуть число или пустую строку, если числ
        }

        public void GetLength()
        {
            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (element.DetailType is CustomElement.Detail.Pipe || element.DetailType is CustomElement.Detail.FlexPipe) 
                    {
                        double pipeLength = Convert.ToDouble(element.ModelLength.Split()[0]);
                        branch.Length += pipeLength;
                    }
                }
            }
        }

        public CustomCollection OrderByLength(Autodesk.Revit.DB.Document doc)
        {
           
            var sorted = Collection
                 .OrderByDescending(branch => branch.Length)   // от большего к меньшему
                 .ToList();
            CustomCollection newCollection = new CustomCollection(doc, sorted);
            return newCollection;
        }
    }
}
