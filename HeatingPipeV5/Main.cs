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

            ObservableCollection<SystemNumber> sysNums = new ObservableCollection<SystemNumber>();
            foreach (var systemnumber in systemnumbers)
            {
                SystemNumber system = new SystemNumber(systemnumber);
                sysNums.Add(system);
            }
            var sortedSysNums = new ObservableCollection<SystemNumber>(sysNums.OrderBy(x => x.SystemName));

            sysNums = sortedSysNums;


            UserControl1 window = new UserControl1();
            MainViewModel mainViewModel = new MainViewModel(doc, window, sysNums);
            
            window.DataContext = mainViewModel;
            window.ShowDialog();

            if (mainViewModel.StartFunction)
            {
                List<ElementId> elIds = new List<ElementId>();
                var systemnames = mainViewModel.SystemNumbersList.Select(x => x).Where(x => x.IsSelected == true);
                //var systemelements = mainViewModel.SystemElements;


                List<ElementId> startelements = new List<ElementId>();
                List<ElementId> selectedterminals = new List<ElementId>();
                List<ElementId> selectedelements = new List<ElementId>();

                foreach (var systemname in systemnames)
                {
                    string systemName = systemname.SystemName;

                    //var maxpipe = GetStartDuct(doc, systemName);

                    selectedterminals = GetMechanicalEquipment(doc, systemName);
                    CustomCollection collection = GetCollection(doc, selectedterminals);
                    //uIDocument.Selection.SetElementIds(collection.ShowElements());


                    collection.Calcualate(mainViewModel.Density);
                    collection.ResCalculate();
                    CustomBranch selectedbranch = collection.SelectMainBranch();
                    //uIDocument.Selection.SetElementIds(selectedbranch.ShowElements());

                    collection.MarkCollection(selectedbranch);
                    string content = collection.GetContent();
                    collection.SaveFile(content);

                }
            }
            else
            {
                List<ElementId> startelements = GetSelectedStartElements(uIDocument);
                startelements = GetMechanicalEquipment(doc, startelements);
                CustomCollection collection = GetCollection(doc, startelements);
                collection.Calcualate(mainViewModel.Density);
                collection.ResCalculate();
                CustomBranch selectedBranch = collection.SelectMainBranch();
                collection.MarkCollection(selectedBranch);
                string content = collection.GetContent();
                collection.SaveFile(content);
            }

            return Result.Succeeded;
        }
        private CustomCollection GetCollection(Document doc, List<ElementId> selectedterminals)
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
        private List<ElementId> GetMechanicalEquipment(Document doc, string systemName)
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
        private List<ElementId> GetMechanicalEquipment(Document doc,List<ElementId> elementIds)
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
