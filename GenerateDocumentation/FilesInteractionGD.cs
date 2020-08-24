using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateDocumentation
{
    static class FilesInteractionGD
    {
        /// <summary>
        /// Чтение текстового файла, в котором содержаться имена параметров и фалимилии. Разделителем служит символ (:).
        /// </summary>
        /// <param name="path">Строка.Путь к расположению текстового файла</param>
        /// <returns>Возвращает словарь из строк. В котором содержаться key - свойства для имени параметра, value - его значения</returns>
        public static List<RevitHelper> ReadtxtFile(string path)
        {
            List<RevitHelper> tempList = new List<RevitHelper>();

            if (path != null)
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(":"))
                        {
                            string[] arr = line.Split(':');
                            tempList.Add(new RevitHelper
                            {
                                KeyWord = arr[0],
                                ParamName = arr[1],
                                ParamValue = arr[1]
                            });

                        }
                    }
                }
            }
            return tempList;
        }

        public static List<RevitHelper> ReadSurnameFile(string path)
        {
            List<RevitHelper> tempList = new List<RevitHelper>();

            if (path != null)
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(":"))
                        {
                            string[] arr = line.Split(':');
                            tempList.Add(new RevitHelper
                            {
                                SurnameKey = arr[0],
                                KeyWord = arr[1],
                                ParamValue = arr[2]
                            });

                        }
                    }
                }
            }
            return tempList;
        }


    }
}
