using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace GenerateDocumentation
{
    public partial class MainForm : Form
    {
        private string kitNumber = "NPP_Номер_Комплекта";
        private string _surnamePath = Properties.Settings.Default.SetSurnamePath;
        public string SurnamePath
        {
            get => _surnamePath;
            set
            {
                _surnamePath = value;
                Properties.Settings.Default.SetSurnamePath = _surnamePath;
            }
        }
        private string _parametersPath = Properties.Settings.Default.SetParametersPath;

        public string ParametersPath
        {
            get => _parametersPath;
            set
            {
                _parametersPath = value;
                Properties.Settings.Default.SetParametersPath = _parametersPath;
            }
        }
        private string _wordTemplatePath = Properties.Settings.Default.SetWordTemplatePath;
        public string WordTemplatePath
        {
            get => _wordTemplatePath;
            set
            {
                _wordTemplatePath = value;
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
                Properties.Settings.Default.SetSaveToFolderPath = _saveToFolderPath;
            }
        }

        private int _numberCab = Properties.Settings.Default.NumberCab;
        public int NumberCab
        {
            get => _numberCab;
            set
            {
                _numberCab = value;
                Properties.Settings.Default.NumberCab = _numberCab;
            }
        }


        private Document _revitDocument;
        WinFormsCommand wf = new WinFormsCommand();
        public MainForm(Document document)
        {
            _revitDocument = document;
            InitializeComponent();


        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RevitGD revit = new RevitGD(_revitDocument);
            comboBox1.Items.AddRange(revit.GetViewSheetSets(kitNumber));
            //label2.Text = SurnamePath;
            //label3.Text = ParametersPath;
            //label6.Text = WordTemplatePath;
            //label8.Text = SaveToFolderPath;
            numericUpDown1.Value = NumberCab;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SurnamePath = wf.SelectPath();
            //label2.Text = SurnamePath;
            Properties.Settings.Default.SetParametersPath = SurnamePath;
            Properties.Settings.Default.Save();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ParametersPath = wf.SelectPath();
            //label3.Text = ParametersPath;
            Properties.Settings.Default.SetParametersPath = ParametersPath;
            Properties.Settings.Default.Save();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            WordTemplatePath = wf.SelectFolder();
            //label6.Text = WordTemplatePath;
            Properties.Settings.Default.SetWordTemplatePath = WordTemplatePath;
            Properties.Settings.Default.Save();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveToFolderPath = wf.SelectFolder();
            //label8.Text = SaveToFolderPath;
            Properties.Settings.Default.SetSaveToFolderPath = SaveToFolderPath;
            Properties.Settings.Default.Save();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Close();
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            if (SurnamePath.Contains(".txt")
                && ParametersPath.Contains(".txt")
                && SaveToFolderPath.Contains(@"\")
                && WordTemplatePath.Contains(@"\")
                && !string.IsNullOrEmpty(comboBox1.Text))
            {
                NumberCab = (int)numericUpDown1.Value;
                Properties.Settings.Default.NumberCab = NumberCab;
                Properties.Settings.Default.Save();
                string selectedviewSheetSet = comboBox1.Text;
                var list1 = RevitGD.GetSheetsName(kitNumber, selectedviewSheetSet);
                var readParameters = FilesInteractionGD.ReadtxtFile(ParametersPath);
                IEnumerable<IGrouping<string, RevitHelper>> repList = RevitGD.GetParametersFromSheets(readParameters,
                    selectedviewSheetSet, kitNumber, list1, NumberCab);

                List<string> pathList = Directory.GetFiles(WordTemplatePath).ToList();
                WordGD wd = new WordGD(pathList);
                string mes = null;
                mes +=
                    "ВНИМАНИЕ!" +
                    "\nРабота плагина остановлена так как не все листы имеют номер (который начинается с CDB,CTB,CAB)" +
                    "\nНеобходимо сначала заполнить данный параметр, затем запустить плагин заново" +
                    "\nЛисты, в которых отсутствует необходимый параметр:\n\n";

                var err = repList
                    .Select(y => y.Where(x => x.KeyWord.Contains("{_CTB_N_}") && !x.ParamValue.Contains("CTB")))
                    .Select(y => y.Where(x => x.KeyWord.Contains("{_CTB_N_}") && !x.ParamValue.Contains("CDB")))
                    .Select(y => y.Where(x => x.KeyWord.Contains("{_CTB_N_}") && !x.ParamValue.Contains("CAB")));
                int c = 0;
                foreach (var parKey in repList)
                {
                    foreach (var par in parKey)
                    {
                        if (par != null)
                        {
                            mes += $"{par.SheetId}    {par.SheetName}    {par.ParamName}    {par.ParamValue}\n";
                            c++;
                        }
                    }
                }
                TaskDialog.Show("R", mes);
                if (c > 0)
                {
                    TaskDialog.Show("ERROR! Заполнение листов!", "sss");

                }
                else
                {
                    List<RevitHelper> readSurnames = FilesInteractionGD.ReadSurnameFile(SurnamePath);
                    //wd.AppendDocument(firstDoc, pathList, SaveToFolderPath, repList, readSurnames);
                    //wd.GenerateULDocumentation(SaveToFolderPath, repList, readSurnames);
                }
            }

            else
            {
                string isnull = null;
                if (!SurnamePath.Contains(".txt"))
                {
                    isnull = "Файл с фамилиями не выбран. Для продолжения необходимо выбрать файл с фамилиями";
                }

                if (!ParametersPath.Contains(".txt"))
                {
                    isnull =
                        "Файл с параметрами не выбран. Для продолжения необходимо выбрать файл с параметрами";
                }

                if (!SaveToFolderPath.Contains(@"\"))
                {
                    isnull = "Не указана папка сохранения файла. Для продолжения необходимо выбрать папку";
                }

                if (!WordTemplatePath.Contains(@"\"))
                {
                    isnull = "Не указана папка с шаблонами Word. Для продолжения необходимо выбрать папку";
                }
                if (String.IsNullOrEmpty(comboBox1.Text))
                {
                    isnull = "Не выбран комплект. Выберите комплект из выпадающего списка";
                }
                TaskDialog.Show("ERROR! Заполнение путей", isnull);
            }
            sw.Stop();
            MessageBox.Show((sw.ElapsedMilliseconds/1000.0).ToString());


        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
