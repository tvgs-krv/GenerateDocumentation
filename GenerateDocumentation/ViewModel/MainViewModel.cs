using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Autodesk.Revit.UI;
using GenerateDocumentation.View;

namespace GenerateDocumentation.ViewModel
{
    public class MainViewModel : ModelBase
    {
        public MainViewModel()
        {
            NumericUpDownViewModelProp = new NumericUpDownViewModel();
            SettingWindowViewModelProp = new SettingWindowViewModel();
            AxelerodOrTitul = "ПКК для Акселерода";
            ParameterName = SettingWindowViewModelProp.ParameterName;
        }
        internal RevitGD RevitModel { get; set; }

        #region Combobox Kit Name
        private List<string> _kitList;
        public List<string> KitList
        {
            get => _kitList;
            set
            {
                _kitList = value;
                OnPropertyChanged(nameof(KitList));
            }
        }

        private string _selectedKit;
        public string SelectedKit
        {
            get => _selectedKit;
            set
            {
                _selectedKit = value;
                OnPropertyChanged(nameof(SelectedKit));
            }
        }

        private string _parameterName;
        public string ParameterName
        {
            get => _parameterName;
            set
            {
                _parameterName = value;
                OnPropertyChanged(nameof(ParameterName));
            }
        }

        #endregion

        #region Settings Button

        private SettingWindowViewModel _settingWindowViewModelProp;
        public SettingWindowViewModel SettingWindowViewModelProp
        {
            get => _settingWindowViewModelProp;
            set
            {
                _settingWindowViewModelProp = value;
                OnPropertyChanged(nameof(SettingWindowViewModelProp));
            }
        }

        private string _surnamePath;
        public string SurnamePath
        {
            get => _surnamePath;
            set
            {
                _surnamePath = value;
                OnPropertyChanged(nameof(SurnamePath));
            }
        }

        private string _parametersPath;
        public string ParametersPath
        {
            get => _parametersPath;
            set
            {
                _parametersPath = value;
                OnPropertyChanged(nameof(ParametersPath));
            }
        }

        private string _wordTemplatePath;
        public string WordTemplatePath
        {
            get => _wordTemplatePath;
            set
            {
                _wordTemplatePath = value;
                OnPropertyChanged(nameof(WordTemplatePath));
            }
        }

        private string _saveToFolderPath;
        public string SaveToFolderPath
        {
            get => _saveToFolderPath;
            set
            {
                _saveToFolderPath = value;
                OnPropertyChanged(nameof(SaveToFolderPath));
            }
        }

        private ICommand _settingsCommand;
        public ICommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand = new RelayCommand(o =>
                {
                    var settingWindow = new SettingWindow();
                    settingWindow.DataContext = SettingWindowViewModelProp;
                    settingWindow.ShowDialog();
                }));
            }
        }

        #endregion

        #region NumericUpDownControl
        private NumericUpDownViewModel _numericUpDownViewModelProp;
        public NumericUpDownViewModel NumericUpDownViewModelProp
        {
            get => _numericUpDownViewModelProp;
            set
            {
                _numericUpDownViewModelProp = value;
                OnPropertyChanged(nameof(NumericUpDownViewModelProp));
            }
        }

        private int _numberCab;
        public int NumberCab
        {
            get => _numberCab;
            set
            {
                _numberCab = value;
                OnPropertyChanged(nameof(NumberCab));
            }
        }
        #endregion

        private string _translater = Properties.Settings.Default.Translater;
        public string Translater
        {
            get => _translater;
            set
            {
                _translater = value;
                OnPropertyChanged(nameof(Translater));
                Properties.Settings.Default.Translater = _translater;
            }
        }

        #region Button OK
        private ICommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(o =>
                {
                    SurnamePath = SettingWindowViewModelProp.SurnamePath;
                    ParametersPath = SettingWindowViewModelProp.ParametersPath;
                    WordTemplatePath = SettingWindowViewModelProp.WordTemplatePath;
                    SaveToFolderPath = SettingWindowViewModelProp.SaveToFolderPath;
                    NumberCab = NumericUpDownViewModelProp.Number;

                    if (SurnamePath.Contains(".txt")
                        && ParametersPath.Contains(".txt")
                        && SaveToFolderPath.Contains(@"\")
                        && WordTemplatePath.Contains(@"\")
                        && !string.IsNullOrEmpty(SelectedKit))
                    {
                        var list1 = RevitGD.GetSheetsName(ParameterName, SelectedKit);
                        var readParameters = FilesInteractionGD.ReadtxtFile(ParametersPath);
                        IEnumerable<IGrouping<string, RevitHelper>> repList = RevitGD.GetParametersFromSheets(readParameters,
                            SelectedKit, ParameterName, list1, NumberCab);
                        List<RevitHelper> unfillSheet = RevitGD.GetUnFillSheets(readParameters, SelectedKit, ParameterName);

                        if (unfillSheet.Count!=0)
                        {
                            string mes = null;
                            mes +=
                                "ВНИМАНИЕ!" +
                                "\nРабота плагина остановлена так как не все листы имеют номер (который начинается с CDB,CTB,CAB)" +
                                "\nНеобходимо сначала заполнить данный параметр, затем запустить плагин заново" +
                                "\nЛисты, в которых отсутствует необходимый параметр:\n\n";
                            foreach (var v in unfillSheet)
                            {
                                mes += $"{v.SheetId}    {v.SheetName}    {v.ParamName}    {v.ParamValue}\n";
                            }
                            TaskDialog.Show("Sheet error", mes);
                        }
                        else
                        {
                            var ulTemplate = Directory.GetFiles(WordTemplatePath + "\\UL").ToList();
                            var wd = new WordGD(ulTemplate);
                            List<RevitHelper> readSurnames = FilesInteractionGD.ReadSurnameFile(SurnamePath);
                            wd.GenerateULDocumentation(SaveToFolderPath, repList, readSurnames);
                            //string mm = null;
                            //foreach (var v in Directory.GetFiles(WordTemplatePath + "\\UL"))
                            //{
                            //    mm += v + "\n";
                            //}
                            //TaskDialog.Show("Sheet error", mm);


                        }
                    }

                    else
                    {
                        string isnull = null;
                        if (!SurnamePath.Contains(".txt"))
                            isnull = "Файл с фамилиями не выбран. Для продолжения необходимо выбрать файл с фамилиями";

                        if (!ParametersPath.Contains(".txt"))
                            isnull =
                                "Файл с параметрами не выбран. Для продолжения необходимо выбрать файл с параметрами";

                        if (!SaveToFolderPath.Contains(@"\"))
                            isnull = "Не указана папка сохранения файла. Для продолжения необходимо выбрать папку";

                        if (!WordTemplatePath.Contains(@"\"))
                            isnull = "Не указана папка с шаблонами Word. Для продолжения необходимо выбрать папку";
                        if (String.IsNullOrEmpty(SelectedKit))
                            isnull = "Не выбран комплект. Выберите комплект из выпадающего списка";
                        TaskDialog.Show("ERROR! Заполнение путей", isnull);
                    }
                }));
            }
        }
        #endregion

        #region RadioButton

        private ChangeRadioButton rb = ChangeRadioButton.Rd;

        public ChangeRadioButton RbRadioButton
        {
            get => rb;
            set
            {
                if (rb == value) return;
                rb = value;
                OnPropertyChanged("RD");
                OnPropertyChanged("BOR");
            }
        }

        public bool IsRd
        {
            get => RbRadioButton == ChangeRadioButton.Rd;
            set => RbRadioButton = value ? ChangeRadioButton.Rd : RbRadioButton;
        }

        public bool IsBop
        {
            get => RbRadioButton == ChangeRadioButton.Bop;
            set => RbRadioButton = value ? ChangeRadioButton.Bop : RbRadioButton;
        }

        private ICommand _isRdCommand;
        public ICommand IsRdCommand => _isRdCommand ?? (_isRdCommand = new RelayCommand(o => { ChangeName(IsRd); }));

        private ICommand _isBopCommand;
        public ICommand IsBopCommand => _isBopCommand ?? (_isBopCommand = new RelayCommand(o => { ChangeName(IsRd); }));
        #endregion

        #region CheckBox RD

        private ICommand _exportAllRdCommand;
        public ICommand ExportAllRdCommand => _exportAllRdCommand ?? (_exportAllRdCommand = new RelayCommand(o => { SelectAll(ExportAllRd); }));

        private ICommand _exportKsRdCommand;
        public ICommand ExportKsRdCommand => _exportKsRdCommand ?? (_exportKsRdCommand = new RelayCommand(o => { SelectNotAll(ExportKSRd); }));

        private ICommand _exportUlRdCommand;
        public ICommand ExportUlRdCommand => _exportUlRdCommand ?? (_exportUlRdCommand = new RelayCommand(o => { SelectNotAll(ExportULRd); }));


        private ICommand _exportAxelerodRdCommand;
        public ICommand ExportAxelerodRdCommand => _exportAxelerodRdCommand ?? (_exportAxelerodRdCommand = new RelayCommand(o => { SelectNotAll(ExportAxelerodRd); }));

        private ICommand _exportSeminaRdCommand;
        public ICommand ExportSeminaRdCommand => _exportSeminaRdCommand ?? (_exportSeminaRdCommand = new RelayCommand(o => { SelectNotAll(ExportSeminaRd); }));

        private ICommand _exportScanRequestRdCommand;
        public ICommand ExportScanRequestRdCommand => _exportScanRequestRdCommand ?? (_exportScanRequestRdCommand = new RelayCommand(o => { SelectNotAll(ExportScanRequestRd); }));

        private bool? _exportAllRd = false;
        public bool? ExportAllRd
        {
            get => _exportAllRd;
            set
            {
                _exportAllRd = value;
                OnPropertyChanged(nameof(ExportAllRd));
            }
        }

        private bool _exportKSRd;
        public bool ExportKSRd
        {
            get => _exportKSRd;
            set
            {
                _exportKSRd = value;
                OnPropertyChanged(nameof(ExportKSRd));
            }
        }

        private bool _exportULRd;
        public bool ExportULRd
        {
            get => _exportULRd;
            set
            {
                _exportULRd = value;
                OnPropertyChanged(nameof(ExportULRd));
            }
        }

        private bool _exportAxelerodRd;
        public bool ExportAxelerodRd
        {
            get => _exportAxelerodRd;
            set
            {
                _exportAxelerodRd = value;
                OnPropertyChanged(nameof(ExportAxelerodRd));
            }
        }

        private bool _exportSeminaRd;
        public bool ExportSeminaRd
        {
            get => _exportSeminaRd;
            set
            {
                _exportSeminaRd = value;
                OnPropertyChanged(nameof(ExportSeminaRd));
            }
        }

        private bool _exportScanRequestRd;
        public bool ExportScanRequestRd
        {
            get => _exportScanRequestRd;
            set
            {
                _exportScanRequestRd = value;
                OnPropertyChanged(nameof(ExportScanRequestRd));
            }
        }

        private string _axelerodOrTitul;
        public string AxelerodOrTitul
        {
            get => _axelerodOrTitul;
            set
            {
                _axelerodOrTitul = value;
                OnPropertyChanged(nameof(AxelerodOrTitul));
            }
        }

        #endregion

        #region Methods
        public void SelectAll(bool? isChecked)
        {
            if (isChecked != null)
            {
                if (ExportAllRd == true)
                {
                    ExportKSRd = true;
                    ExportULRd = true;
                    ExportAxelerodRd = true;
                    ExportSeminaRd = true;
                    ExportScanRequestRd = true;
                }
                if (isChecked == false)
                {
                    ExportKSRd = false;
                    ExportULRd = false;
                    ExportAxelerodRd = false;
                    ExportSeminaRd = false;
                    ExportScanRequestRd = false;
                }

            }
        }
        public void SelectNotAll(bool isChecked)
        {
            if (isChecked) ExportAllRd = null;

            if (!isChecked) ExportAllRd = false;

            if (ExportKSRd && ExportULRd && ExportAxelerodRd && ExportSeminaRd && ExportScanRequestRd)
            {
                ExportAllRd = true;
            }
        }
        public void ChangeName(bool isRd)
        {
            if (isRd) AxelerodOrTitul = "ПКК для Акселерода";
            if (!isRd) AxelerodOrTitul = "Титульный блок ВОР";
        }
        #endregion
    }

    public enum ChangeRadioButton
    {
        Rd,
        Bop
    }



}
