using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenerateDocumentation
{
    class WinFormsCommand
    {
        /// <summary>
        /// Открытие диалогового окна для получения пути к файлу
        /// </summary>
        /// <returns>Если файл выбран, возвращает путь в строковом формате, если нет,null</returns>
        public string SelectPath()
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
        public string SelectFolder()
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
