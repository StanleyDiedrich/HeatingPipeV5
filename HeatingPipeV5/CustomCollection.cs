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

            customBranch.CreateNewBranch(Document, airterminal);
            CustomElement customElement = new CustomElement(Document,  airterminal);
            Collection.Add(customBranch);


        }
        public CustomCollection(Autodesk.Revit.DB.Document doc)
        {
            Document = doc;

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
                foreach (var element in branch.Elements)
                {

                    if (element.DetailType == CustomElement.Detail.Equipment)
                    {
                        if (element.ElementId.IntegerValue == 5807241)
                        {
                            var element2 = element;
                        }
                        branch.Pressure += 8000;
                        branch.Length += 0;
                        
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
                        //CustomElbow customElbow = new CustomElbow(Document, element);
                        //element.LocRes = customElbow.LocRes;
                        //element.PDyn = Density * Math.Pow(customElbow.Velocity, 2) / 2 * element.LocRes;
                        branch.Pressure += 15;
                    }
                    else if (element.DetailType == CustomElement.Detail.Tee)
                    {
                        if (element.ElementId.IntegerValue == 6253444)
                        {
                            var element2 = element;
                        }
                        CustomTee customTee = new CustomTee(Document, element);
                        element.LocRes = customTee.LocRes;
                       
                        //CustomTee customTee = new CustomTee(Document, element);
                        //element.LocRes = customTee.LocRes;
                        //element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
                        branch.Pressure += 20;
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
                        if (element.ElementId.IntegerValue == 5981916)
                        {
                            var element2 = element;
                        }
                        try
                        {
                            //CustomTransition customTransition = new CustomTransition(Document, element);

                            /*element.LocRes = customTransition.LocRes;
                            element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;*/
                        }
                        catch
                        {
                            element.LocRes = 0.5;
                        }
                        branch.Pressure += 5;
                    }
                   
                    else if (element.DetailType == CustomElement.Detail.Pipe )
                    {
                        if (element.ElementId.IntegerValue == 2808562)
                        {
                            ElementId el = element.ElementId;
                        }
                            CustomPipe customPipe = new CustomPipe(Document, element);
                        element.Ptot = customPipe.Pressure;
                        branch.Pressure += customPipe.Pressure;
                        /*branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PIPE_PRESSUREDROP_PARAM).AsDouble();
                        string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PIPE_PRESSUREDROP_PARAM).AsValueString().Split();
                        element.PStat = double.Parse(pressureDropString[0], formatter);*/
                    }
                    else if (element.DetailType == CustomElement.Detail.Valve)
                    {
                        branch.Pressure += 10000;
                    }
                    else if (element.DetailType == CustomElement.Detail.Union)
                    {
                        branch.Pressure += 0;
                    }
                    else if (element.OwnConnectors.Size>2)
                    {
                        element.DetailType = CustomElement.Detail.Manifold;
                        branch.Pressure += 5000;
                    }
                }
            }
        }
        public void ResCalculate()
        {
            foreach (var branch in Collection)
            {
                branch.PBTot = 0;

                for (int i = 1; i < branch.Elements.Count; i++)
                {

                    branch.Elements[i].Ptot = branch.Elements[i].PDyn + branch.Elements[i].PStat + branch.Elements[i - 1].Ptot;


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
            var maxbranch = branches.OrderByDescending(x => x.Pressure).FirstOrDefault();
            return maxbranch;
        }

        public void MarkCollection(CustomBranch customBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();

           



            // Сначала обрабатываем основную ветвь 
            foreach (var branch in Collection)
            {
                if (branch.Number == customBranch.Number)
                {
                    int trackCounter = 0;
                    
                    foreach (var element in branch.Elements)
                    {
                        element.GroupNumber = 0;
                        element.TrackNumber = trackCounter;
                        element.BranchNumber = branch.Number;
                        element.MainTrack = true;
                        checkedElements.Add(element.ElementId);
                        trackCounter++;
                    }
                    newCustomCollection.Add(branch);
                    break; // Прекращаем дальнейший обход после нахождения основной ветви 
                }
            }
            
            // Обрабатываем остальные ветви 
            foreach (var branch in Collection)
            {
                if (branch.Number == customBranch.Number)
                {
                    continue;
                }

                CustomBranch newCustomBranch = new CustomBranch(Document);
                int trackCounter = 0;

                foreach (var element in branch.Elements)
                {
                    // Если элемент уже есть в основной ветви, пропускаем его 
                    if (checkedElements.Contains(element.ElementId))
                    {
                        continue;
                    }

                    // Устанавливаем номера и добавляем элемент в новую ветвь 
                    element.TrackNumber = trackCounter;
                    element.BranchNumber = branch.Number;
                    newCustomBranch.Add(element);
                    checkedElements.Add(element.ElementId);
                    trackCounter++;  // Увеличиваем trackCounter только после успешного добавления элемента
                }

                newCustomCollection.Add(newCustomBranch);
            }
            int groupnumber = 0;
            foreach (var branch in newCustomCollection)
            {
                var pipe = branch.Elements.Select(x => x).Where(x => x.DetailType == CustomElement.Detail.Equipment).First();
                string sysname = pipe.Element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
                string sysname1 = sysname.Split(',')[0];
                string sysname2 = sysname.Split(',')[1];

                foreach (var branch2 in newCustomCollection)
                {
                    foreach (var element in branch2.Elements)
                    {
                        if (element.DetailType == CustomElement.Detail.Manifold)
                        {
                            continue;
                        }
                        else if (element.Element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString().Equals(sysname1) || element.Element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString().Equals(sysname2))
                        {
                            element.GroupNumber = groupnumber;
                        }
                         if (element.BranchNumber==customBranch.Number)
                        {
                            element.GroupNumber = groupnumber;
                        }
                    }
                }
                groupnumber++;

            }
            // Обновляем коллекцию 
            Collection = newCustomCollection;
        }



        public string GetContent()
        {
            var csvcontent = new StringBuilder();
            csvcontent.AppendLine("ElementId;DetailType;Name;SystemName;Level;GroupNumber;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");

            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    string a = $"{element.ElementId};{element.DetailType};{element.ElementName};{element.SystemName};{element.Lvl};{element.GroupNumber};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelWidth};{element.ModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.ModelVelocity};{element.PStat};{element.LocRes};{element.PDyn};{element.Ptot};" +
                         $"{element.SystemName}-{element.Lvl}-{element.GroupNumber}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";
                    csvcontent.AppendLine(a);
                }
            }

            return csvcontent.ToString();
        }
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
    }
}
