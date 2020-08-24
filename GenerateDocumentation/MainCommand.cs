using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GenerateDocumentation.View;
using GenerateDocumentation.ViewModel;

namespace GenerateDocumentation
{
    [Transaction(TransactionMode.Manual)]
    public class MainCommand : IExternalCommand
    {
        public static MainWindow MainWindow { get; private set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (MainWindow == null)
                {
                    var uiApp = commandData.Application;
                    var uidoc = uiApp.ActiveUIDocument;
                    var doc = uidoc.Document;
                    MainViewModel mvvm = new MainViewModel();
                    var revitModel = new RevitGD(doc);
                    mvvm.RevitModel = revitModel;
                    
                    mvvm.KitList = revitModel.GetViewSheetSets_2(mvvm.ParameterName);
                    MainWindow = new MainWindow { DataContext = mvvm };
                    MainWindow.Closed += (sender, args) => MainWindow = null;
                    MainWindow.ShowDialog();
                }
                else
                {
                    MainWindow.Activate();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

}
