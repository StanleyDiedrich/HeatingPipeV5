using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Win32;

namespace HeatingPipeV5
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Main : IExternalCommand
    {
        static AddInId AddInId = new AddInId(new Guid("D17A5AF1-40C3-4C1D-B0CA-7802DFFD12E6"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uIDocument = uiapp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uIDocument.Document;

            List<string> systemnumbers = new List<string>();
            List<string> modelNames = new List<string>();

            var linkInstances = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .ToList();

            IList<Element> pipes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsNotElementType().ToElements();

            foreach (Element pipe in pipes)
            {
                var newpipe = pipe as Pipe;

                try
                {
                    if (newpipe != null)
                    {
                        if (!systemnumbers.Contains(newpipe.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString()))
                        {
                            systemnumbers.Add(newpipe.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Revit", ex.ToString());
                }
            }


            
            foreach (var model in linkInstances )
            {
                try
                {
                    if (model!=null)
                    {
                        if (!modelNames.Contains(model.Name))
                        {
                            modelNames.Add(model.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Revit", ex.ToString());
                }

            }
            ObservableCollection<ModelsName> modelsNames = new ObservableCollection<ModelsName>();
            foreach (var modName in modelNames)
            {
                ModelsName model = new ModelsName(modName);
                modelsNames.Add(model);
            }
            var sortedModNames = new ObservableCollection<ModelsName>(modelsNames.OrderBy(x => x.ModelName));
            modelsNames = sortedModNames;




            ObservableCollection<SystemNumber> sysNums = new ObservableCollection<SystemNumber>();
            foreach (var systemnumber in systemnumbers)
            {
                SystemNumber system = new SystemNumber(systemnumber);
                sysNums.Add(system);
            }
            var sortedSysNums = new ObservableCollection<SystemNumber>(sysNums.OrderBy(x => x.SystemName));

            sysNums = sortedSysNums;


            UserControl1 window = new UserControl1();
            MainViewModel mainViewModel = new MainViewModel(doc, window, sysNums, modelsNames);
            
            window.DataContext = mainViewModel;
            window.ShowDialog();


            switch (mainViewModel.StartFunction)
            {
                case Regime.TOTAL:
                    {
                        var systemNames = mainViewModel.SystemNumbersList
                            .Where(x => x.IsSelected)
                            .Select(x => x.SystemName);

                        foreach (var systemName in systemNames)
                        {
                            var selectedTerminals = GetMechanicalEquipment(doc, systemName);
                            var collection = GetCollection(doc, selectedTerminals);

                            collection.Calcualate(mainViewModel.Density);
                            collection.ResCalculate();

                            var selectedBranch = collection.SelectMainBranch();

                            collection.MarkCollection(selectedBranch);
                            var content = collection.PrepareContent();
                            collection.SaveFile(content);
                        }
                        break;
                    }

                case Regime.PARTIAL:
                    {
                        var startElements = GetSelectedStartElements(uIDocument);
                        if (startElements != null && startElements.Count > 0)
                        {
                            startElements = GetMechanicalEquipment(doc, startElements);
                            var collection = GetCollection(doc, startElements);

                            collection.Calcualate(mainViewModel.Density);
                            collection.ResCalculate();

                            var selectedBranch = collection.SelectMainBranch();

                            collection.MarkCollection(selectedBranch);
                            var content = collection.PrepareContent();
                            collection.SaveFile(content);
                        }
                        break;
                    }

                case Regime.MEP_ROOM_COLLECTION:
                {
                        List<Element> mep_rooms = new List<Element>();
                       var selectedModel = mainViewModel.ModelsList.Where(x => x.IsSelected).Select(x => x.ModelName).ToList();
                       FilteredElementCollector filter = new FilteredElementCollector(doc);
                       var linkedElement = filter.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToList();
                        foreach (var model in selectedModel)
                       {
                            foreach (var  linkmodel in linkedElement)
                            {
                                if (linkmodel.Name.Equals(model))
                                {
                                    FilteredElementCollector filter1 = new FilteredElementCollector(linkmodel.Document);
                                    var activedocument = (doc.GetElement(linkmodel.Id) as RevitLinkInstance).GetLinkDocument();
                                    FilteredElementCollector linkedFilter = new FilteredElementCollector(activedocument);
                                    mep_rooms = linkedFilter.OfCategory(BuiltInCategory.OST_MEPSpaces).WhereElementIsNotElementType().ToList();
                                    


                                }
                            }
                       }
                        
                        break;
                }

                default:
                    // необязательная обработка по умолчанию
                    break;
            }




            return Result.Succeeded;
        }
        private CustomCollection GetCollection(Autodesk.Revit.DB.Document doc, List<ElementId> selectedterminals)
        {
            CustomCollection collection = new CustomCollection(doc);
            foreach (var terminal in selectedterminals)
            {
                collection.CreateBranch(doc, terminal);
            }

            return collection;
        }

        private List<ElementId> GetSelectedStartElements(UIDocument uIDocument)
        {
            IList<Reference> selectedRefs = uIDocument.Selection.PickObjects(ObjectType.Element, "Выберите элементы");

            // Получаем элементы из ссылок
            List<Element> elements = selectedRefs.Select(r => uIDocument.Document.GetElement(r)).ToList();
            List<ElementId> elementIds = new List<ElementId>();
            elementIds = elements.Select(x => x.Id).ToList();
           
            return elementIds;
        }
        private List<ElementId> GetMechanicalEquipment(Autodesk.Revit.DB.Document doc, string systemName)
        {
            List<ElementId> resultterminals = new List<ElementId>();
            var terminals = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().ToElementIds().ToList();
            foreach (var terminal in terminals)
            {
                if (terminal.IntegerValue == 5982031)
                {
                    var airterminal2 = terminal;
                }
                if (doc.GetElement(terminal) != null)
                {
                    FamilyInstance fI = doc.GetElement(terminal) as FamilyInstance;
                    if (fI != null)
                    {
                        var checksystem = fI.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
                        if (checksystem == null)
                        {
                            continue;
                        }
                        else if (checksystem.Contains(systemName))
                        {
                            resultterminals.Add(terminal);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return resultterminals;
        }
        private List<ElementId> GetMechanicalEquipment(Autodesk.Revit.DB.Document doc,List<ElementId> elementIds)
        {
            List<ElementId> resultterminals = new List<ElementId>();

            foreach (var elementId in elementIds)
            {
                Element element = doc.GetElement(elementId);
                if (element.Category.Id.IntegerValue== (int)BuiltInCategory.OST_MechanicalEquipment)
                {
                    resultterminals.Add(element.Id);
                }
            }
           
            return resultterminals;
        }
    }
}
