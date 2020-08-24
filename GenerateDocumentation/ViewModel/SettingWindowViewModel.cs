using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace GenerateDocumentation.ViewModel
{
    public class SettingWindowViewModel : ModelBase
    {
        private string _surnamePath = Properties.Settings.Default.SetSurnamePath;
        public string SurnamePath
        {
            get => _surnamePath;
            set
            {
                _surnamePath = value;
                OnPropertyChanged(nameof(SurnamePath));
                Properties.Settings.Default.SetSurnamePath = _surnamePath;
                Properties.Settings.Default.Save();
            }
        }

        private string _parameterName = Properties.Settings.Default.SetParameterName;
        public string ParameterName
        {
            get => _parameterName;
            set
            {
                _parameterName = value;
                OnPropertyChanged(nameof(ParameterName));
                Properties.Settings.Default.SetParameterName = _parameterName;
                Properties.Settings.Default.Save();
            }
        }


        private string _parametersPath = Properties.Settings.Default.SetParametersPath;
        public string ParametersPath
        {
            get => _parametersPath;
            set
            {
                _parametersPath = value;
                OnPropertyChanged(nameof(ParametersPath));
                Properties.Settings.Default.SetParametersPath = _parametersPath;
                Properties.Settings.Default.Save();
            }
        }

        private string _wordTemplatePath = Properties.Settings.Default.SetWordTemplatePath;
        public string WordTemplatePath
        {
            get => _wordTemplatePath;
            set
            {
                _wordTemplatePath = value;
                OnPropertyChanged(nameof(WordTemplatePath));
                Properties.Settings.Default.SetWordTemplatePath = _wordTemplatePath;
            }
        }

        private string _saveToFolderPath = Properties.Settings.Default.SetSaveToFolderPath;
        public string SaveToFolderPath
        {
            get => _saveToFolderPath;
            set
            {
                _saveToFolderPath = value;
                OnPropertyChanged(nameof(SaveToFolderPath));
                Properties.Settings.Default.SetSaveToFolderPath = _saveToFolderPath;
            }
        }

        private ICommand _selectPathCommand;
        public ICommand SelectPathCommand
        {
            get
            {
                return _selectPathCommand ?? (_selectPathCommand = new RelayCommand(o =>
                {
                    ParametersPath = SelectPath();
                    Properties.Settings.Default.SetParametersPath = ParametersPath;
                    Properties.Settings.Default.Save();
                }));
            }
        }

        private ICommand _selectSurnameCommand;
        public ICommand SelectSurnameCommand
        {
            get
            {
                return _selectSurnameCommand ?? (_selectSurnameCommand = new RelayCommand(o =>
                {
                    SurnamePath = SelectPath();
                    Properties.Settings.Default.SetSurnamePath = SurnamePath;
                    Properties.Settings.Default.Save();
                }));
            }
        }

        private ICommand _selectTemplateCommand;
        public ICommand SelectTemplateCommand
        {
            get
            {
                return _selectTemplateCommand ?? (_selectTemplateCommand = new RelayCommand(o =>
                {
                    WordTemplatePath = SelectFolder();
                    Properties.Settings.Default.SetWordTemplatePath = WordTemplatePath;
                    Properties.Settings.Default.Save();
                }));
            }
        }

        private ICommand _selectSavePathCommand;
        public ICommand SelectSavePathCommand
        {
            get
            {
                return _selectSavePathCommand ?? (_selectSavePathCommand = new RelayCommand(o =>
                {
                    SaveToFolderPath = SelectFolder();
                    Properties.Settings.Default.SetSaveToFolderPath = SaveToFolderPath;
                    Properties.Settings.Default.Save();
                }));
            }
        }

        /// <summary>
        /// Открытие диалогового окна для получения пути к файлу
        /// </summary>
        /// <returns>Если файл выбран, возвращает путь в строковом формате, если нет,null</returns>
        private string SelectPath()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    return openFileDialog.FileName;
                }
            }
            return null;

        }
        /// <summary>
        /// Открытие диалогового окна для получения пути к папке
        /// </summary>
        /// <returns>Если папка выбрана, возвращает путь в строковом формате, если нет,null</returns>
        private string SelectFolder()
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    return folderBrowserDialog.SelectedPath;
                }
            }
            return null;
        }




    }
}
