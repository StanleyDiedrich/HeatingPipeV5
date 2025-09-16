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
using System.Windows;
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
            Autodesk.Revit.Creation.Document createDoc = doc.Create;

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
            ObservableCollection<Worksheet> worksheets = new ObservableCollection<Worksheet>();
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
            MainViewModel mainViewModel = new MainViewModel(doc, window, sysNums, modelsNames, worksheets);
            
            window.DataContext = mainViewModel;
            window.ShowDialog();


            switch (mainViewModel.StartFunction)
            {
                case Regime.TOTAL:
                    {
                       /* var systemNames = mainViewModel.SystemNumbersList
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
                        }*/
                        break;
                    }

                case Regime.PARTIAL:
                    {
                        /*var startElements = GetSelectedStartElements(uIDocument);
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
                        }*/
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

                        var workset = mainViewModel.WorksheetList.Where(x => x.IsSelected)
                            .Select(x => x.WorksheetName)
                            .FirstOrDefault(); ;
                        if (workset==null)
                        {
                             workset = mainViewModel.WorksheetList
                            .Select(x => x.WorksheetName)
                            .FirstOrDefault(name => name == "(30)_Отопление");


                        }

                        foreach (var systemName in systemNames)
                        {
                           
                            var mep_equipment = GetMechanicalEquipment(doc, systemName,workset);
                            var collection = GetCollection(doc, mep_equipment);

                            collection.Calcualate(mainViewModel.Density);
                            collection.MarkPipes();
                            collection.Shift();
                            //var loops = collection.GetNewLoopBeforeManifold();
                            //collection.GetLength();
                            //collection.OrderByLength(doc);
                            //collection.MarkBranches();
                            /*collection.ResCalculate();
                            var selectedBranch = collection.SelectMainBranch();
                            collection.MarkCollection(selectedBranch);*/
                            //collection.OrderTracksByManifold();
                            //collection.OrderTracks();
                            //collection.RemoveDuplicates();

                            // это надо
                            List<DanfossPipe> danfossElements = collection.GetDanfossElements();
                            List<DanfossEquipment> danfossEquipment = collection.GetDanfossEquipment();
                            List<DanfossManifold> danfossManifolds = collection.GetDanfossManifolds();
                            //это надо



                            SaveFile(doc, danfossElements,danfossEquipment, danfossManifolds);
                            //var content = collection.GetContent();
                            //collection.SaveFile(content);
                        }
                        
                        
                    break;
                }
                case Regime.CHANGE_SYSTEM_NAME:
                {
                        var systemNames = mainViewModel.SystemNumbersList
                           .Where(x => x.IsSelected)
                           .Select(x => x.SystemName);

                        FilteredElementCollector filteredSystems = new FilteredElementCollector(doc);
                        var fSystems = filteredSystems.OfCategory(BuiltInCategory.OST_PipingSystem).WhereElementIsNotElementType().ToList(); // отфильтрованные системы

                       

                        foreach (var  system in fSystems) //перебор по отфильтрованным системам 
                        {
                            foreach (var sysName in systemNames) // проход по выбранным системам 
                            {
                                var systemType = system.GetTypeId();
                                if (doc.GetElement(systemType).get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM).AsString().Equals(sysName))
                                {
                                    string systemName = doc.GetElement(systemType).get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM).AsString();
                                    var param = system.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
                                    string abbreviation = param.Split()[0];
                                    string name = param.Split()[1];
                                    string newName = sysName + " " + name;

                                    using (Transaction t = new Transaction(doc, "Переименование системы"))
                                    {
                                        t.Start();
                                        try
                                        {
                                            system.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).Set(newName);
                                            t.Commit();
                                        }
                                        catch
                                        { }
                                    }
                                }
                            }
                           
                        }

                        break;
                }
                case Regime.COPY_LINKED_MEP_SPACE:
                {
                        List<Element> mep_rooms = new List<Element>();
                        var selectedModel = mainViewModel.ModelsList.Where(x => x.IsSelected).Select(x => x.ModelName).ToList();
                        FilteredElementCollector filter = new FilteredElementCollector(doc);
                        var linkedElement = filter.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToList();

                        Workset selectedWorset = mainViewModel.WorksheetList.Where(x => x.IsSelected).Select(x => x.Workset).First();

                        foreach (var model in selectedModel)
                        {
                            foreach (var linkmodel in linkedElement)
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
                        List<CustomSpace> copiedSpaces = new List<CustomSpace>();
                        foreach (var mepRoom in mep_rooms)
                        {
                            if (mepRoom!=null)
                            {
                                CustomSpace customSpace = new CustomSpace(doc, mepRoom);
                                if(customSpace.Location!=null || customSpace.Name!=null || customSpace.Temperature!=null ||customSpace.HeatLoading!=null ||customSpace.Level!=null  )
                                {
                                    copiedSpaces.Add(customSpace);
                                }
                               
                            }
                            
                        }

                        Phase targetPhase = new FilteredElementCollector(doc)
                        .OfClass(typeof(Phase))
                        .Cast<Phase>()
                        .FirstOrDefault();
                        foreach (var copiedSpace in copiedSpaces)
                        {

                            using (Transaction t = new Transaction(doc,"CreateSpace"))
                            {
                                t.Start();
                                try
                                {
                                    Space newSpace = createDoc.NewSpace(copiedSpace.Level, targetPhase, copiedSpace.Location);
                                    newSpace.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NAME).Set(copiedSpace.Number);
                                    //newSpace.get_Parameter(BuiltInParameter.ROOM_NAME).Set(copiedSpace.Name);
                                    newSpace.LookupParameter("ADSK_Номер квартиры").Set(copiedSpace.Name);
                                    newSpace.LookupParameter("ADSK_Температура в помещении").Set(copiedSpace.Temperature);
                                    newSpace.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).Set(copiedSpace.HeatLoading);
                                    t.Commit();
                                }
                                catch
                                {
                                    t.RollBack();
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


        private void SaveFile(Autodesk.Revit.DB.Document doc, List<DanfossPipe> danfossPipes , List<DanfossEquipment> danfossEquipment, List<DanfossManifold> danfossManifolds)
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
                    for (int c = 1; c <= 22; c++) worksheet.Column(c).Width = 15;
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
                    //worksheet.Cells[1, 23].Value = "Число отводов";



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
                       /* foreach(var elbow in danfossElbows)
                        {
                            if(elbow.Mark.Equals(r.Comment))
                            {
                                var elbows = danfossElbows.GroupBy(x => x.Mark).Select(x => x).Max(x => x.Count());
                                worksheet.Cells[row, 23].Value = elbow.Count;
                            }
                            break;
                        }*/



                       




                       
                    }

                    var worksheet_eq = package.Workbook.Worksheets.Add("Оборудование");

                    for (int c = 1; c <= 20; c++) worksheet.Column(c).Width = 15;
                    worksheet_eq.Cells[1, 1].Value = "Id";
                    worksheet_eq.Cells[1, 2].Value = "Система";
                    worksheet_eq.Cells[1, 3].Value = "Тип";
                    worksheet_eq.Cells[1, 4].Value = "Символ";
                    worksheet_eq.Cells[1, 5].Value = "n/L";
                    worksheet_eq.Cells[1, 6].Value = "Фрг";
                    worksheet_eq.Cells[1, 7].Value = "Разм";
                    worksheet_eq.Cells[1, 8].Value = "Укр";
                    worksheet_eq.Cells[1, 9].Value = "Lмакс";
                    worksheet_eq.Cells[1, 10].Value = "a";
                    worksheet_eq.Cells[1, 11].Value = "Подключение";
                    worksheet_eq.Cells[1, 12].Value = "Уровень";
                    worksheet_eq.Cells[1, 13].Value = "Помещение";
                    worksheet_eq.Cells[1, 14].Value = "Подключение";
                    worksheet_eq.Cells[1, 15].Value = "dT";
                    worksheet_eq.Cells[1, 16].Value = "Axo";
                    worksheet_eq.Cells[1, 17].Value = "Coc";
                    worksheet_eq.Cells[1, 18].Value = "Комментарии";
                    worksheet_eq.Cells[1, 19].Value = "Производитель";
                    worksheet_eq.Cells[1, 20].Value = "Описание";

                    for (int i =0; i<danfossEquipment.Count;i++)
                    {
                        var r = danfossEquipment[i];
                        int row = i + 2;

                        worksheet_eq.Cells[row, 1].Value = r.Id;
                        worksheet_eq.Cells[row, 2].Value = r.System;
                        worksheet_eq.Cells[row, 3].Value = r.Type;
                        worksheet_eq.Cells[row, 4].Value = r.Symbol;
                        worksheet_eq.Cells[row, 5].Value = r.n_L;
                        worksheet_eq.Cells[row, 6].Value = r.Frg;
                        worksheet_eq.Cells[row, 7].Value = r.Size;
                        worksheet_eq.Cells[row, 8].Value = r.Cover;
                        worksheet_eq.Cells[row, 9].Value = r.LengthMax;
                        worksheet_eq.Cells[row, 10].Value = r.Alpha;
                        worksheet_eq.Cells[row, 11].Value = r.Connection;
                        worksheet_eq.Cells[row, 12].Value = r.Lvl;
                        worksheet_eq.Cells[row, 13].Value = r.Room;
                        worksheet_eq.Cells[row, 14].Value = r.Connection2;
                        worksheet_eq.Cells[row, 15].Value = r.dT;
                        worksheet_eq.Cells[row, 16].Value = r.Axo;
                        worksheet_eq.Cells[row, 17].Value = r.Status;
                        worksheet_eq.Cells[row, 18].Value = r.Comment;
                        worksheet_eq.Cells[row, 19].Value = r.Manufacturer;
                        worksheet_eq.Cells[row, 20].Value = r.Description;
                    }
                    var worksheet_man = package.Workbook.Worksheets.Add("Коллекторы");

                    for (int c = 1; c <= 15; c++) worksheet.Column(c).Width = 15;
                    worksheet_man.Cells[1, 1].Value = "Id";
                    worksheet_man.Cells[1, 2].Value = "Тип системы";
                    worksheet_man.Cells[1, 3].Value = "Тип";
                    worksheet_man.Cells[1, 4].Value = "Символ";
                    worksheet_man.Cells[1, 5].Value = "DN";
                    worksheet_man.Cells[1, 6].Value = "Tmix";
                    worksheet_man.Cells[1, 7].Value = "dT";
                    worksheet_man.Cells[1, 8].Value = "Количество контуров";
                    worksheet_man.Cells[1, 9].Value = "Уровень";
                    worksheet_man.Cells[1, 10].Value = "Уровень2";
                    worksheet_man.Cells[1, 11].Value = "Ахо";
                    worksheet_man.Cells[1, 12].Value = "Состояние";
                    worksheet_man.Cells[1, 13].Value = "Комментарий";
                    worksheet_man.Cells[1, 14].Value = "Производитель";
                    worksheet_man.Cells[1, 15].Value = "Описание";

                    for (int i=0;i<danfossManifolds.Count;i++)
                    {
                        var r = danfossManifolds[i];
                        int row = i + 2;

                        worksheet_man.Cells[row, 1].Value = r.Id;
                        worksheet_man.Cells[row, 2].Value = r.System;
                        worksheet_man.Cells[row, 3].Value = r.Type;
                        worksheet_man.Cells[row, 4].Value = r.Symbol;
                        worksheet_man.Cells[row, 5].Value = r.Dn;
                        worksheet_man.Cells[row, 6].Value = r.Tmix;
                        worksheet_man.Cells[row, 7].Value = r.dT;
                        worksheet_man.Cells[row, 8].Value = r.Contours;
                        worksheet_man.Cells[row, 9].Value = r.Lvl;
                        worksheet_man.Cells[row, 10].Value = r.Lvl2;
                        worksheet_man.Cells[row, 11].Value = r.Axo;
                        worksheet_man.Cells[row, 12].Value = r.Condition;
                        worksheet_man.Cells[row, 13].Value = r.Comment;
                        worksheet_man.Cells[row, 14].Value = r.Manufacturer;
                        worksheet_man.Cells[row, 15].Value = r.Description;
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
        private List<ElementId> GetMechanicalEquipment(Autodesk.Revit.DB.Document doc, string systemName, string workSetName)
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
                        if (fI.MEPModel.ConnectorManager==null)
                        {
                            continue;
                        }
                        if(fI.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).AsValueString().Equals(workSetName))
                        {
                            try
                            {
                                var checksystem = fI.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();

                                string abbreviation = checksystem.Split()[0];
                                if (checksystem == null)
                                {
                                    continue;
                                }
                                else if (checksystem.Contains(abbreviation))
                                {
                                    resultterminals.Add(terminal);
                                }
                            }
                            catch (Exception ex)
                            {
                                TaskDialog.Show("Оборудование не имеет системы", $"{terminal}");
                            }
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
