using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Xceed.Words.NET;

namespace GenerateDocumentation
{
    class WordGD
    {
        private DocX _titleDoc;
        private DocX _cabDoc;
        private DocX _cdbDoc;
        private DocX _ctbDoc;
        private string _titlePath;
        public WordGD(List<string> wordTemplateFolder)
        {
            _titlePath = wordTemplateFolder.FirstOrDefault(x => x.Contains("TITLE") && !x.Contains(@"~$"));
            string cabPath = wordTemplateFolder.FirstOrDefault(x => x.Contains("CAB") && !x.Contains(@"~$"));
            string cdbPath = wordTemplateFolder.FirstOrDefault(x => x.Contains("CDB") && !x.Contains(@"~$"));
            string ctbPath = wordTemplateFolder.FirstOrDefault(x => x.Contains("CTB") && !x.Contains(@"~$"));
            _cabDoc = DocX.Load(cabPath);
            _cdbDoc = DocX.Load(cdbPath);
            _ctbDoc = DocX.Load(ctbPath);
        }

        public WordGD()
        {
            
        }

        /// <summary>
        /// Insert a document at the end of another document.
        /// </summary>
        public void AppendDocument(string pathFirstDocument, List<string> pathSecondDocument, string saveThirdDocument, IEnumerable<IGrouping<string, RevitHelper>> paramList, List<RevitHelper> surnameList)
        {
            string gg = null;

            try
            {
                using (var document1 = DocX.Load(_titlePath))
                {
                    List<IGrouping<string, RevitHelper>> enumerable = paramList.ToList();
                    var title = surnameList
                        .Where(x => x.SurnameKey.Contains("TITLE"));
                    var surcab = surnameList.Where(x => x.SurnameKey.Contains("CAB"));
                    var prList = enumerable.FirstOrDefault().ToList();
                    var prList2 = enumerable.FirstOrDefault().ToList();
                    prList.AddRange(title);
                    prList2.AddRange(surcab);
                    foreach (var v in prList)
                    {
                        document1.ReplaceText(v.KeyWord, v.ParamValue);
                    }

                    string getcabPath = pathSecondDocument.FirstOrDefault(x => x.Contains("CAB") && !x.Contains(@"~$"));
                    if (getcabPath != null)
                    {
                        using (var document2 = DocX.Load(getcabPath))
                        {
                            document1.InsertDocument(document2, true);
                            foreach (var v in prList2)
                            {
                                document1.ReplaceText(v.KeyWord, v.ParamValue);
                            }
                        }
                    }


                    string getName = string.Empty;
                    foreach (IGrouping<string, RevitHelper> parKey in enumerable)
                    {
                        getName = parKey
                            .Where(p => p.KeyWord.Contains("{_КОМПЛЕКТ_}"))
                            .Select(p => p.ParamValue).FirstOrDefault();
                        var checkCdb = parKey.Any(p => p.ParamValue.Contains("CDB"));
                        var checkCtb = parKey.Any(p => p.ParamValue.Contains("CTB"));
                        var checkCab = parKey.Any(p => p.ParamValue.Contains("CAB"));
                        string getPath = string.Empty;
                        List<RevitHelper> rh = new List<RevitHelper>();
                        IEnumerable<RevitHelper> sur = new List<RevitHelper>();
                        if (checkCab)
                        {
                            getPath = pathSecondDocument.FirstOrDefault(x => x.Contains("CAB") && !x.Contains(@"~$"));
                            sur = surnameList.Where(x => x.SurnameKey.Contains("CAB"));
                        }

                        if (checkCtb)
                        {
                            getPath = pathSecondDocument.FirstOrDefault(x => x.Contains("CTB") && !x.Contains(@"~$"));
                            sur = surnameList.Where(x => x.SurnameKey.Contains("CTB"));

                        }

                        if (checkCdb)
                        {
                            getPath = pathSecondDocument.FirstOrDefault(x => x.Contains("CDB") && !x.Contains(@"~$"));
                            sur = surnameList.Where(x => x.SurnameKey.Contains("CDB"));

                        }

                        rh.AddRange(sur);
                        rh.AddRange(parKey);
                        gg += getPath + "\n";
                        if (!string.IsNullOrEmpty(getPath))
                        {




                            using (var document2 = DocX.Load(getPath))
                            {
                                document1.InsertParagraph().InsertPageBreakAfterSelf();
                                document1.InsertDocument(document2, true);
                                foreach (RevitHelper v in rh)
                                {
                                    document1.ReplaceText(v.KeyWord, v.ParamValue);
                                    gg += $"{v.SheetName}    {v.KeyWord}    {v.ParamName}    {v.ParamValue}\n";
                                }
                            }

                        }

                    }

                    //TaskDialog.Show("r", gg);
                    try
                    {
                        document1.SaveAs($"{saveThirdDocument}\\{getName}.docx");

                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Revit Error",
                            "Невозможно сохранить файл, так как он открыть. Закройте файл и запустите по новой");

                    }
                }
                TaskDialog.Show("Revit Error", "ГОТОВО");
            }
            catch (ArgumentNullException ex)
            {
                TaskDialog.Show("Revit Error", $"{ex.Message}\n Не выбран комплект в выпадающем списке.");
            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit Error", $"{e.Message}\n");
            }
        }




        public void GenerateULDocumentation(string savePath, IEnumerable<IGrouping<string, RevitHelper>> paramList, List<RevitHelper> surnameList)
        {
            try
            {
                using (var document1 = DocX.Load(_titlePath))
                {
                    var list = paramList as IGrouping<string, RevitHelper>[] ?? paramList.ToArray();
                    var surnameForTitle = surnameList.Where(x => x.SurnameKey.Contains("TITLE"));
                    var surnameForCab = surnameList.Where(x => x.SurnameKey.Contains("CAB"));
                    var surnameForCdb = surnameList.Where(x => x.SurnameKey.Contains("CDB"));
                    var surnameForCtb = surnameList.Where(x => x.SurnameKey.Contains("CTB"));
                    var prList = list.FirstOrDefault().ToList();
                    var prList2 = list.FirstOrDefault().ToList();
                    prList.AddRange(surnameForTitle);
                    prList2.AddRange(surnameForCab);
                    foreach (var v in prList)
                    {
                        document1.ReplaceText(v.KeyWord, v.ParamValue);
                    }
                    document1.InsertDocument(_cabDoc, true);
                    foreach (var v in prList2)
                    {
                        document1.ReplaceText(v.KeyWord, v.ParamValue);
                    }

                    string getName = string.Empty;
                    foreach (IGrouping<string, RevitHelper> parKey in list)
                    {
                        getName = parKey
                            .Where(p => p.KeyWord.Contains("{_КОМПЛЕКТ_}"))
                            .Select(p => p.ParamValue).FirstOrDefault();
                        var checkCdb = parKey.Any(p => p.ParamValue.Contains("CDB"));
                        var checkCtb = parKey.Any(p => p.ParamValue.Contains("CTB"));
                        var checkCab = parKey.Any(p => p.ParamValue.Contains("CAB"));
                        List<RevitHelper> rh = new List<RevitHelper>();
                        rh.AddRange(parKey);
                        if (checkCab)
                        {
                            rh.AddRange(surnameForCab);
                            document1.InsertParagraph().InsertPageBreakAfterSelf();
                            document1.InsertDocument(_cabDoc, true);
                        }

                        if (checkCtb)
                        {
                            rh.AddRange(surnameForCtb);
                            document1.InsertParagraph().InsertPageBreakAfterSelf();
                            document1.InsertDocument(_ctbDoc, true);
                        }

                        if (checkCdb)
                        {
                            rh.AddRange(surnameForCdb);
                            document1.InsertParagraph().InsertPageBreakAfterSelf();
                            document1.InsertDocument(_cdbDoc, true);
                        }
                        foreach (RevitHelper v in rh)
                        {
                            document1.ReplaceText(v.KeyWord, v.ParamValue);
                        }
                    }
                    try
                    {
                        document1.SaveAs($"{savePath}\\{getName}.docx");
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Revit Error",
                            "Невозможно сохранить файл, так как он открыть. Закройте файл и запустите плагин заново");
                    }

                }
            }
            catch (ArgumentNullException ex)
            {
                TaskDialog.Show("Revit Error", $"{ex.Message}\n Не выбран комплект в выпадающем списке.");
            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit Error", $"{e.Message}\n");
            }
        }
    }

}
