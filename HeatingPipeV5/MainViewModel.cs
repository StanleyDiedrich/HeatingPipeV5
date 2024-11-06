using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Input;
using HeatingPipeV5;

namespace HeatingPipeV5
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SystemNumber> _systemNumbersList;

        //public ObservableCollection<CalculationMode> CalculationModes { get; set; }
        public ObservableCollection<Workset> WorkSets { get; set; } = new ObservableCollection<Workset>();

        private double _temperature = 20;
        public double Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged("Temperature");
                OnPropertyChanged("Density");
            }
        }
        private double density = 1.21;
        public double Density
        {
            get
            {
                // Вычисляем Density на основе Temperature
                return 353 / (273 + _temperature);
            }

        }
        private Workset _selectedWorkSet;
        public Workset SelectedWorkSet
        {
            get => _selectedWorkSet;
            set
            {
                _selectedWorkSet = value;
                OnPropertyChanged("SelectedWorkSet");
            }
        }
        private SystemNumber _selectedSystemNumber;
        private Autodesk.Revit.DB.Document document;
        public Autodesk.Revit.DB.Document Document
        {
            get { return document; }
            set
            {
                document = value;
                OnPropertyChanged("Document");
            }
        }

        private UserControl1 window;
        public UserControl1 Window
        {
            get { return window; }
            set
            {
                window = value;
                OnPropertyChanged("Window");
            }
        }
        private string _searchText = "Выберите систему";
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(FilteredSystemNumbersList));
            }
        }

        public ObservableCollection<SystemNumber> SystemNumbersList
        {
            get => _systemNumbersList;
            set
            {
                _systemNumbersList = value;
                OnPropertyChanged(nameof(SystemNumbersList));
                OnPropertyChanged(nameof(FilteredSystemNumbersList));
            }
        }
        public ObservableCollection<SystemNumber> FilteredSystemNumbersList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchText) || SearchText.Equals("Выберите систему"))
                {
                    return new ObservableCollection<SystemNumber>(SystemNumbersList);
                }

                // Используем ToList() для получения реальной коллекции после фильтрации
                var filteredList = SystemNumbersList
                    .Where(system => !string.IsNullOrWhiteSpace(system.SystemName) &&
                                     system.SystemName.ToLower().Contains(SearchText.ToLower()))
                    .ToList();

                return new ObservableCollection<SystemNumber>(filteredList);
            }
        }
        public SystemNumber SelectedSystemNumber
        {
            get => _selectedSystemNumber;
            set
            {
                _selectedSystemNumber = value;
                OnPropertyChanged(nameof(SelectedSystemNumber));
            }
        }


        private string _selectedSystems;
        public string SelectedSystems
        {
            get => _selectedSystems;
            set
            {
                _selectedSystems = value;
                OnPropertyChanged(nameof(SelectedSystems));
            }
        }

        public ICommand ShowSelectedSystemsCommand { get; }

        public void ShowSelectedSystems(object param)
        {


            //var foundedelements = GetElements(Document, SystemNumbersList);
            //SystemElements = GetSystemElements(foundedelements);
        }
        public ICommand StartCommand { get; }

        public void StartCalculate(object param)
        {
            var selectedItems = SystemNumbersList.Where(x => x.IsSelected).Select(x => x.SystemName).ToList();
            SelectedSystems = string.Join(", ", selectedItems);
            Window.Close();
        }

        private List<SystemElement> systemElements;
        public List<SystemElement> SystemElements
        {
            get { return systemElements; }
            set
            {
                systemElements = value;
                OnPropertyChanged("SystemElements");
            }
        }








        public List<SystemElement> GetSystemElements(List<Element> elements)
        {
            List<SystemElement> systemElements = new List<SystemElement>();
            foreach (var element in elements)
            {
                SystemElement systemElement = new SystemElement(element);
                systemElements.Add(systemElement);
            }
            return systemElements;
        }

        /*public void UpdateSelectedMode()
        {
            // Сброс флага IsMode для всех режимов
            foreach (var mode in CalculationModes)
            {
                mode.IsMode = false;
            }

            // Установить IsMode для выбранного режима
            var selectedMode = CalculationModes.FirstOrDefault(m => m.IsMode);
            // Другие действия с выбранным режимом
        }
*/





        public MainViewModel(Autodesk.Revit.DB.Document doc, UserControl1 window, ObservableCollection<SystemNumber> systemNumbers)
        {
            Window = window;
            Document = doc;
            SystemNumbersList = systemNumbers;
            FilteredWorksetCollector collector = new FilteredWorksetCollector(doc);
            IList<Workset> worksets = collector.OfKind(WorksetKind.UserWorkset).ToWorksets();
            foreach (var workset in worksets)
            {
                WorkSets.Add(workset);
            }


            ShowSelectedSystemsCommand = new RelayCommand(ShowSelectedSystems);
            StartCommand = new RelayCommand(StartCalculate);
            /*CalculationModes = new ObservableCollection<CalculationMode>
        {
            new CalculationMode { CalculationName = "Обход системы вентиляции", CalculationId=0, IsMode = false },

        };*/
            // SystemElements = new List<SystemElement>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
