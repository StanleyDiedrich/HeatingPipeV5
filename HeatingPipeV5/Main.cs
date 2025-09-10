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
using OfficeOpenXml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
                       var danfossRooms= GetDanfossRooms(mep_rooms);
                        SaveFile(doc, danfossRooms);
                       
                        
                        break;
                }
                case Regime.MEP_HEATING_COLLECTION:
                {
                        var systemNames = mainViewModel.SystemNumbersList
                            .Where(x => x.IsSelected)
                            .Select(x => x.SystemName);

                        var mep_rooms = GetMepRooms(doc);
                        
                        

                        foreach (var systemName in systemNames)
                        {
                            var mep_equipment = GetMechanicalEquipment(doc, systemName);
                            var collection = GetCollection(doc, mep_equipment);

                            collection.Calcualate(mainViewModel.Density);
                            collection.GetLength();
                            collection.OrderByLength(doc);
                            collection.MarkPipes();
                            //collection.MarkBranches();
                            /*collection.ResCalculate();

                            var selectedBranch = collection.SelectMainBranch();

                            collection.MarkCollection(selectedBranch);*/
                            //var content = collection.GetContent();
                            List<DanfossPipe> danfossElements = collection.GetDanfossElements();
                           
                            SaveFile(doc, danfossElements );
                            //collection.SaveFile(content);
                        }
                        
                        
                    break;
                }
                default:
                    // необязательная обработка по умолчанию
                    break;
            }




            return Result.Succeeded;
        }

       

        

        private List<Element> GetMepRooms(Autodesk.Revit.DB.Document doc)
        {
            List<Element> mep_rooms = new List<Element>();

            FilteredElementCollector filter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces);
            mep_rooms = filter.WhereElementIsNotElementType().ToList();

            return mep_rooms;
        }

        private List<DanfossRoom> GetDanfossRooms (List<Element> mep_rooms)
        {
            List<DanfossRoom> danfoss_rooms = new List<DanfossRoom>();
            foreach (var mep_room in mep_rooms)
            {
                if (mep_room!=null /*|| mep_room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble()==0*/)
                {
                    if(mep_room.Id.IntegerValue==5410085)
                    {
                        var meproom1 = mep_room;
                    }
                    try
                    {
                        DanfossRoom danfossRoom = new DanfossRoom(mep_room);
                        danfoss_rooms.Add(danfossRoom);
                    }
                    catch (Exception ex)
                    {
                       TaskDialog.Show("Ошибка создания пространства", ex.ToString());
                    }
                   
                }
                
            }

            return danfoss_rooms;
        }
        private void SaveFile(Autodesk.Revit.DB.Document doc, List<DanfossRoom> danfossRooms)
        {
            if (danfossRooms == null || danfossRooms.Count == 0) return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Выберите папку для сохранения документа",
                FileName = doc.Title,
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName; // полный путь выбранного файла

            ExcelPackage.License.SetNonCommercialOrganization("Министерство нехороших дел");

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Пространства");
                    for (int c = 1; c <= 11; c++) worksheet.Column(c).Width = 15;
                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 60;

                    
                    worksheet.Cells[1, 1].Value = "Символ";
                    worksheet.Cells[1, 2].Value = "A";
                    worksheet.Cells[1, 3].Value = "Θ+intH";
                    worksheet.Cells[1, 4].Value = "Θ+intC";
                    worksheet.Cells[1, 5].Value = "ΦHL";
                    worksheet.Cells[1, 6].Value = "ΦHG";
                    worksheet.Cells[1, 7].Value = "A1p";
                    worksheet.Cells[1, 8].Value = "ΦCL";
                    worksheet.Cells[1, 9].Value = "Комнатный термостат";
                    worksheet.Cells[1, 10].Value = "Описание";
                    worksheet.Cells[1, 11].Value = "Комментарии";

                    for (int i = 0; i < danfossRooms.Count; i++)
                    {
                        var r = danfossRooms[i];
                        int row = i + 2;
                        
                        worksheet.Cells[row, 1].Value = r.Symbol;
                        worksheet.Cells[row, 2].Value = r.Area;
                        worksheet.Cells[row, 3].Value = r.TempInt;
                        worksheet.Cells[row, 4].Value = r.TempOut;
                        worksheet.Cells[row, 5].Value = r.HeatLoad;
                        worksheet.Cells[row, 6].Value = r.HeatAuto;
                        worksheet.Cells[row, 7].Value = r.Area1P;
                        worksheet.Cells[row, 8].Value = r.ColdLoad;
                        worksheet.Cells[row, 9].Value = r.RoomTermostate;
                        worksheet.Cells[row, 10].Value = r.Description;
                        worksheet.Cells[row, 11].Value = r.Comment;
                    }

                    package.SaveAs(new FileInfo(filePath));
                }
            }
            catch (Exception ex)
            {
                // Логирование или уведомление пользователя
                System.Windows.MessageBox.Show("Ошибка при сохранении Excel: " + ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }


        private void SaveFile(Autodesk.Revit.DB.Document doc, List<DanfossPipe> danfossPipes)
        {
            if (danfossPipes == null || danfossPipes.Count == 0) return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Выберите папку для сохранения документа",
                FileName = doc.Title+"_Трубы",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName; // полный путь выбранного файла

            ExcelPackage.License.SetNonCommercialOrganization("Министерство нехороших дел");

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Трубы");
                    for (int c = 1; c <= 21; c++) worksheet.Column(c).Width = 15;
                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 60;

                    worksheet.Cells[1, 1].Value = "Ids";
                    worksheet.Cells[1, 2].Value = "Тип";
                    worksheet.Cells[1, 3].Value = "Труба";
                    worksheet.Cells[1, 4].Value = "Тип трубы";
                    worksheet.Cells[1, 5].Value = "Стояк";
                    worksheet.Cells[1, 6].Value = "Участок";
                    worksheet.Cells[1, 7].Value = "Dn";
                    worksheet.Cells[1, 8].Value = "Изоляция";
                    worksheet.Cells[1, 9].Value = "Тизо";
                    worksheet.Cells[1, 10].Value = "Длина";
                   
                   
                    worksheet.Cells[1, 11].Value = "Ост";
                    worksheet.Cells[1, 12].Value = "Уровень";
                    worksheet.Cells[1, 13].Value = "Помещение";

                    worksheet.Cells[1, 14].Value = "Отв";
                    worksheet.Cells[1, 15].Value = "О/д";
                    worksheet.Cells[1, 16].Value = "Сос";

                    worksheet.Cells[1, 17].Value = "Комментарии";
                    worksheet.Cells[1, 18].Value = "Символ";
                    worksheet.Cells[1, 19].Value = "Производитель";
                    worksheet.Cells[1, 20].Value = "Описание";
                    worksheet.Cells[1, 21].Value = "Отнач";
                    worksheet.Cells[1, 22].Value = "Откон";




                    for (int i = 0; i < danfossPipes.Count; i++)
                    {
                        var r = danfossPipes[i];
                        int row = i + 2;
                        worksheet.Cells[row, 1].Value = r.Ids;
                        worksheet.Cells[row, 2].Value = r.HeatingSystem;        // Тип
                        worksheet.Cells[row, 3].Value = r.Type;                 // Труба
                        worksheet.Cells[row, 4].Value = r.PipeType;             // Стояк
                        worksheet.Cells[row, 5].Value = r.Riser;                // Участок
                        worksheet.Cells[row, 6].Value = r.Part;                 // Dn
                        worksheet.Cells[row, 7].Value = r.DiameterNominal;      // Изоляция (если поменять местами — проверьте соответствие)
                        worksheet.Cells[row, 8].Value = r.Insulation;           // Тизо
                        worksheet.Cells[row, 9].Value = r.InsulationThick;      // Plc
                        worksheet.Cells[row, 10].Value = r.Length;                // L

                        worksheet.Cells[row, 11].Value = r.Ost;         // Ост
                        worksheet.Cells[row, 12].Value = r.Lvl;             // Уровень
                        worksheet.Cells[row, 13].Value = r.Room;                // Помещение

                        worksheet.Cells[row, 14].Value = r.Otv;               // Отв
                        worksheet.Cells[row, 15].Value = r.Od;          // О/д
                        worksheet.Cells[row, 16].Value = r.Sos;          // Сос

                        worksheet.Cells[row, 17].Value = r.Comment;               // Комментарии
                        worksheet.Cells[row, 18].Value = r.Symbol;              // Символ
                        worksheet.Cells[row, 19].Value = r.Manufacturer;        // Производитель
                        worksheet.Cells[row, 20].Value = r.Description;     // Описание (расширенное)
                        worksheet.Cells[row, 21].Value = r.Onach;          // Отнач
                        worksheet.Cells[row, 22].Value = r.Okonech;            // Откон

                        package.SaveAs(new FileInfo(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование или уведомление пользователя
                System.Windows.MessageBox.Show("Ошибка при сохранении Excel: " + ex.Message, "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
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
