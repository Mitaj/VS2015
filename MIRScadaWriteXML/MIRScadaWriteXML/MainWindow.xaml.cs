using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace MIRScadaWriteXML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private InfoWindow _infWindow;
        private List<TbParamsXml> _mainCollection = new List<TbParamsXml>();
        public BackgroundWorker BackgroundWorker = new BackgroundWorker();
        private Thread _threadProgress;
        private string _mainPath = "D:\\Projects\\ASDU\\Тесты\\MirScadaWriteXML\\НоябрьскВосток\\Replace";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /* _mainCollection.Add(new TbParamsXml { Use = false, ClassName = "Надпись", Param = "BorderStyle", Attr = "textValue", Value = "{param1}", UseP = true,Regexp = "^<<[A-Za-zА-Яа-я0-9_]{1,}>>.", Param1 = "Anchor", Attr1 = "textValue", Param2 = "", Attr2 = "", UseTmp = false, Template = "Group46" });
            _mainCollection.Add(new TbParamsXml { Use = false, ClassName = "Надпись", Param = "BorderStyle", Attr = "textValue", Value = "{param1}", UseP = true, Regexp = ".{1,}", Param1 = "SizeDynamicHor", Attr1 = "textValue", Param2 = "", Attr2 = "", UseTmp = false, Template = "Group46" });
            _mainCollection.Add(new TbParamsXml { Use = true, ClassName = "АСДУЭ.Выключатель", Param = "ТУ", Attr = "textValue", Value = "use(<<ТУ_Стандарт>>;\"{param1}\")", Regexp = "^<<[\\wА-Яа-я]+>>\\.*\\S*", UseP = true, Param1 = "ТУ", Attr1 = "textValue", Param2 = "", Attr2 = "", UseTmp = false, Template = "" });
            _mainCollection.Add(new TbParamsXml { Use = true, ClassName = "АСДУЭ.Выключатель", Param = "ТУ", Attr = "cppclassname", Value = "ArrayProperty", UseP = false, Param1 = "", Attr1 = "textValue", UseTmp = false, Template = "" });
            _mainCollection.Add(new TbParamsXml { Use = true, ClassName = "АСДУЭ.Ключ_АВР", Param = "ТУ", Attr = "cppclassname", Value = "ArrayProperty", UseP = false, Param1 = "", Attr1 = "textValue", UseTmp = false, Template = "" });
            _mainCollection.Add(new TbParamsXml { Use = true, ClassName = "АСДУЭ.Трансф", Param = "ТУ", Attr = "cppclassname", Value = "ArrayProperty", UseP = false, Param1 = "", Attr1 = "textValue", UseTmp = false, Template = "" });
            _mainCollection.Add(new TbParamsXml { Use = true, ClassName = "АСДУЭ.Трансф", Param = "ТУ", Attr = "textValue", Value = "use(<<ТУ_Стандарт>>;\"{param1}\")", Regexp = "^<<[\\wА-Яа-я]+>>\\.*\\S*", UseP = true, Param1 = "ТУ", Attr1 = "textValue", Param2 = "", Attr2 = "", UseTmp = false, Template = "" });
            _mainCollection.Add(new TbParamsXml { Use = false, ClassName = "АСДУЭ.Выключатель", Param = "ЦепьУправления", Attr = "textValue", Value = "{param1}", UseP = true, Param1 = "ТУЕстьОперНапр", Attr1 = "textValue" });
            _mainCollection.Add(new TbParamsXml { Use = false, ClassName = "АСДУЭ.Выключатель", Param = "ЦепьУправления_Доп", Attr = "textValue", Value = "{param1}", UseP = true, Param1 = "ТУ_Доп_ЕстьОперНапр", Attr1 = "textValue" });
           */
            if (DataGrid != null) DataGrid.ItemsSource = _mainCollection;

            TextBox.Text = _mainPath;

            BackgroundWorker.WorkerReportsProgress = true;
            BackgroundWorker.DoWork += backgroundWorker_DoWork;
            BackgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            BackgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProjectParserXml prXml = new ProjectParserXml(_mainPath);
            int current = 0;
            int max = DataGrid.Items.Count;
            foreach (TbParamsXml rowData in DataGrid.ItemsSource)
            {
                current++;
                int progress = (current*100/max);
                BackgroundWorker.ReportProgress(progress);
                System.Threading.Thread.Sleep(100);
                if (rowData.Use)
                {
                    if (rowData.UseP || rowData.UseTmp)
                    {
                        prXml.EditObjectParamsInXmlEx(rowData);
                    }
                    else
                        prXml.EditObjectParamsInXml(rowData.ClassName, rowData.Param, rowData.Attr, rowData.Value, rowData.VRegexp);
                }
            }

        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainProgressBar.Value = 0;
            StatusTextBlock.Text = "Операция замены тегов завершена в папке " + TextBox.Text;
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BackgroundWorker.RunWorkerAsync();
            StatusTextBlock.Text = "Не закрывайте приложение пока не завершится запись в Xml файлы";
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonFolder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;

            DialogResult result = folderBrowserDialog.ShowDialog(new OldWindow(mainWindowPtr));

            if (result.ToString() == "OK")
            {
                TextBox.Text = folderBrowserDialog.SelectedPath;
                _mainPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!BackgroundWorker.IsBusy)
                StatusTextBlock.Text = "";
        }


        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файл настроек(*.txt)|*.txt";
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                fileName = openFileDialog.FileName;


            ImportExportTbParamsXml loadFile = new ImportExportTbParamsXml();
            loadFile.LoadParamsCollection(fileName);
            _mainCollection = loadFile.GetParamsCollection();
            DataGrid.ItemsSource = _mainCollection;
        }


        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файл настроек(*.txt)|*.txt";
            string fileName = "";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    List<TbParamsXml> saveCollection = (List<TbParamsXml>) DataGrid.Items.SourceCollection;
                    fileName = saveFileDialog.FileName;
                    ImportExportTbParamsXml saveFile = new ImportExportTbParamsXml();
                    saveFile.SaveParamsCollection(_mainCollection, fileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Неверные данные", "Ошибка Записи");
                }
               
            }


        }

        private void MenuItemDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGrid != null)
                {
                    TbParamsXml currentItem = (TbParamsXml) DataGrid.CurrentItem;
                    _mainCollection.Remove(currentItem);
                    DataGrid.ItemsSource = _mainCollection;
                    DataGrid.Items.Refresh();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Пустая строка", "Ошибка удаления");
            }
        }

        private void View_Button_Click(object sender, RoutedEventArgs e)
        {
            ProjectParserXml prXml = new ProjectParserXml(_mainPath);
            string writeText = prXml.GetInfoParamsFromFiles((IEnumerable<TbParamsXml>)DataGrid.ItemsSource);
            System.IO.StreamWriter textFile = new System.IO.StreamWriter(@"ViewText.txt");
            textFile.WriteLine(writeText);
            textFile.Close();
            Process.Start("C:\\Windows\\System32\\notepad.exe", "ViewText.txt");
        }
    }
}
