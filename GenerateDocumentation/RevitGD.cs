using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;

namespace GenerateDocumentation
{
    class RevitGD
    {
        private static Document _document;
        public RevitGD(Document document)
        {
            _document = document;
        }
        public RevitGD(Document document, string parameterName)
        {
            _document = document;
            GetViewSheetSets(parameterName);
        }



        /// <summary>
        /// Получение значения параметра, сначали идет поиск по экземпляру, если параметра не существует, то идет по иск по типу, если и здесь парметра не найдено, то возвращается null
        /// </summary>
        /// <param name="element">элемент, у которого необходимо получить параметр</param>
        /// <param name="parameterName"> Наименование параметра</param>
        /// <returns>Возвращает значение парарметра типа Object. Необходима проверка на соответствие типу. Если значения нет, возвращает null</returns>
        private static object GetParameterValue(Element element, string parameterName)
        {
            Parameter parameter;
            try
            {
                parameter = element.LookupParameter(parameterName);
            }
            catch (NullReferenceException)
            {
                var type = _document.GetElement(element.GetTypeId());
                parameter = type.LookupParameter(parameterName);
            }
            catch
            {
                return null;
            }

            object getVal = null;
            if (parameter != null)
            {
                StorageType storType = parameter.StorageType;
                if (storType == StorageType.String)
                    getVal = parameter.AsString();

                if (storType == StorageType.Double)
                    getVal = parameter.AsDouble();

                if (storType == StorageType.Integer)
                    getVal = parameter.AsInteger();

                if (storType == StorageType.ElementId)
                    getVal = parameter.AsElementId();
                return getVal;
            }
            return null;
        }

        /// <summary>
        /// Получение массива наименования наборов листов для печати
        /// </summary>
        /// <returns>Возвращает массив строк</returns>
        public object[] GetViewSheetSets(string parameterName)
        {
            var fec = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Sheets)
                   .WhereElementIsNotElementType()
                   .Where(x => x != null)
                   .Select(x => GetParameterValue(x, parameterName).ToString())
                   .GroupBy(x => x).Select(x => x.First()).ToArray();
            return fec;
        }

        public List<string> GetViewSheetSets_2(string parameterName)
        {
            try
            {
                return new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Sheets)
                    .WhereElementIsNotElementType()
                    .Where(x => x != null)
                    .Select(x => GetParameterValue(x, parameterName).ToString())
                    .GroupBy(x => x).Select(x => x.First()).ToList();

            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit error",
                    "Не удалось получить значения из параметра. Проверьте правильность параметра в настройках");
            }
            return new List<string>();
        }


        private static RevitHelper ReplaceValueDictionary(Element element, RevitHelper paramFromTxt)
        {
            RevitHelper temp;
            var val = (GetParameterValue(element, paramFromTxt.ParamName))?.ToString();
            temp = new RevitHelper
            {
                SheetName = element.Name,
                SheetId = element.Id.IntegerValue,
                KeyWord = paramFromTxt.KeyWord,
                ParamName = paramFromTxt.ParamName,
                ParamValue = val ?? paramFromTxt.ParamName
            };

            return temp;
        }
        private static RevitHelper ReplaceValueSurname(Element element, RevitHelper paramFromTxt)
        {
            RevitHelper temp;
            var val = (GetParameterValue(element, paramFromTxt.ParamName))?.ToString();
            if (val != null)
            {
                temp = new RevitHelper
                {
                    SheetName = element.Name,
                    SheetId = element.Id.IntegerValue,
                    KeyWord = paramFromTxt.KeyWord,
                    ParamName = paramFromTxt.ParamName,
                    ParamValue = val
                };
            }
            else
            {
                temp = new RevitHelper
                {
                    SheetName = element.Name,
                    SheetId = element.Id.IntegerValue,
                    KeyWord = paramFromTxt.KeyWord,
                    ParamName = paramFromTxt.ParamName,
                    ParamValue = paramFromTxt.ParamName
                };
            }

            return temp;
        }

        private static RevitHelper ReplaceValueDictionary(Element element, string value, string keyWord)
        {
            RevitHelper temp;
            if (value != null)
            {

                temp = new RevitHelper
                {
                    SheetName = element.Name,
                    SheetId = element.Id.IntegerValue,
                    KeyWord = keyWord,
                    ParamName = keyWord,
                    ParamValue = value
                };
            }
            else
            {
                temp = new RevitHelper
                {
                    SheetName = element.Name,
                    SheetId = element.Id.IntegerValue,
                    KeyWord = keyWord,
                    ParamName = keyWord,
                    ParamValue = String.Empty
                };
            }
            return temp;
        }

        public static IEnumerable<IGrouping<string, RevitHelper>> GetParametersFromSheets(List<RevitHelper> paramList,
            string equalsString, string kitNumber, List<string> sheetList, int numberCab)
        {
            string gg = null;
            var fec = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType().Where(x => x != null).Select(x => x);

            var proj = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_ProjectInformation)
                .WhereElementIsNotElementType().Where(x => x != null).Select(x => x);
            List<RevitHelper> projList = new List<RevitHelper>();

            foreach (var pr in proj)
            {
                foreach (var par in paramList)
                {
                    var val = ReplaceValueDictionary(pr, par);
                    if (val != null)
                    {
                        projList.Add(val);
                    }
                }


            }

            List<RevitHelper> tempList = new List<RevitHelper>();

            foreach (var el in fec)
            {
                var getValueKit = GetParameterValue(el, kitNumber)?.ToString();
                string sheetFormat = GetSheetsFormat(el.Id);
                if (getValueKit != null && equalsString.Contains(getValueKit))
                {
                    foreach (var par in paramList)
                    {
                        tempList.Add(ReplaceValueDictionary(el, par));
                    }
                    tempList.Add(GetSheetCount(el, sheetList, "{_ЛИСТЫ_}", true, numberCab));
                    tempList.Add(GetSheetCount(el, sheetList, "{_ФОРМАТЫ_}", false, numberCab));
                    tempList.Add(ReplaceValueDictionary(el, DeleteLastCharFromString(sheetFormat), "{_ФОРМАТ_}"));
                    tempList.Add(new RevitHelper
                    {
                        SheetName = el.Name,
                        SheetId = el.Id.IntegerValue,
                        KeyWord = "{_COUNT_SHEET_}",
                        ParamName = "{_COUNT_SHEET_}",
                        ParamValue = numberCab.ToString()
                    });

                    foreach (var p in projList)
                    {
                        tempList.Add(new RevitHelper
                        {
                            SheetName = el.Name,
                            SheetId = el.Id.IntegerValue,
                            KeyWord = p.KeyWord,
                            ParamName = p.ParamName,
                            ParamValue = p.ParamValue
                        });
                    }
                    //foreach (var sur in surnameList)
                    //{
                    //    tempList.Add(ReplaceValueSurname(el, sur));
                    //}
                }
            }

            var group = tempList.Where(x => x.ParamValue != null)
                .Where(f => !f.ParamValue.Equals(f.KeyWord))
                .Where(f => !f.ParamValue.Contains("NPP"))
                .GroupBy(g => new { g.KeyWord, g.SheetId })
                .Select(x => x.First())
                .GroupBy(g => g.SheetId.ToString());
            return group;
        }










        public static List<RevitHelper> GetUnFillSheets(List<RevitHelper> paramList, string equalsString, string kitNumber)
        {
            var fec = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType().Where(x => x != null).Select(x => x);
            List<RevitHelper> tempList = new List<RevitHelper>();
            foreach (var el in fec)
            {
                var getValueKit = GetParameterValue(el, kitNumber)?.ToString();
                string sheetFormat = GetSheetsFormat(el.Id);
                if (getValueKit != null && equalsString.Contains(getValueKit))
                    foreach (var par in paramList)
                    {
                        var replace = ReplaceValueDictionary(el, par);
                        Regex regex1 = new Regex(@"[A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]");
                        if (replace.ParamName.Contains("CDB, CTB") && !regex1.IsMatch(replace.ParamValue))
                            tempList.Add(replace);
                    }
            }
            return tempList;
        }

        public static List<string> GetSheetsName(string kitNumber, string equalsString)
        {
            List<string> sheetList = new List<string>();
            List<string[]> sh = new List<string[]>();

            IEnumerable<Element> titBlocks = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType().Where(x => x != null).Select(x => x);
            foreach (Element titBlock in titBlocks)
            {
                Element type = _document.GetElement(titBlock.OwnerViewId);
                var getValueKit = GetParameterValue(type, kitNumber)?.ToString();

                if (type != null && equalsString.Contains(getValueKit))
                {
                    sheetList.Add(titBlock.Name);
                    sh.Add(new string[] { titBlock.OwnerViewId.ToString(), titBlock.Name });
                }
            }
            return sheetList;
        }
        /// <summary>
        /// Получение формата листа.
        /// </summary>
        /// <param name="idSheet"> ID листа (OST_SHEETS)</param>
        /// <returns>Возвращает строку, которая содержит формат листа</returns>
        public static string GetSheetsFormat(ElementId idSheet)
        {
            string sheetFormat = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_TitleBlocks)
                 .WhereElementIsNotElementType()
                 .Where(x => x != null)
                 .Where(x => idSheet == x.OwnerViewId)
                 .Select(x => x.Name)
                 .FirstOrDefault();
            return sheetFormat;
        }

        private static RevitHelper GetSheetCount(Element element, List<string> sheetList, string keyWord, bool isCount, int cabNumber)
        {
            string tt = null;
            var ll = sheetList.Select(DeleteLastCharFromString).ToList();
            for (int i = 0; i < cabNumber; i++)
            {
                ll.Add("A3");
            }
            if (isCount)
            {
                var result = ll.GroupBy(n => n).Select(m => m.Count());
                tt = string.Join("\n", result);
            }
            else
            {
                tt = string.Join("\n", ll.Distinct());
            }

            return new RevitHelper
            {
                SheetName = element.Name,
                SheetId = element.Id.IntegerValue,
                KeyWord = keyWord,
                ParamName = keyWord,
                ParamValue = tt
            };
        }

        private static string DeleteLastCharFromString(string inputString)
        {
            string s2 = inputString.Substring(inputString.Length - 1);
            return s2.Contains("A") || s2.Contains("А") || s2.Contains("K") || s2.Contains("К")
                ? inputString.Substring(0, inputString.Length - 1)
                : inputString;
        }

    }

    class RevitHelper
    {
        public int SheetId { get; set; }
        public string SheetName { get; set; }
        public string KeyWord { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public string SurnameKey { get; set; }
    }
}
