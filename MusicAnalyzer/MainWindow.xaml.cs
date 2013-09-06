using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using WinForms =System.Windows.Forms;
using System.Windows.Media.Animation;
using SongMarker;

namespace WpfApplication1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeMyApp();
        }
        DataAggregator da = new DataAggregator();
        DataTable dt,dt_temp = new DataTable();
        int CollectionToProcessSize, CurrentProgress;
        bool FilesAddTerminared;

        private void InitializeMyApp()
        {
            da.IsSafeToQuit = true;
            dt = da.GetDataSet().Tables[0];
            dt_temp = da.GetDataSet().Tables[0];
            if (dt.Rows.Count == 0)
                label3.Content = "Your music library is empty now";
            else
                label3.Content = "Your music library contains "+dt.Rows.Count+" titles";
            dataGrid1.ItemsSource = dt_temp.DefaultView;
            int[] NumberOfSongsShow = new int[] { 10, 20, 30, 50, 100 };
            for (int i = 0; i < NumberOfSongsShow.Length; i++)
            {
                comboBox1.Items.Add(NumberOfSongsShow[i]);
            }
            comboBox1.SelectedIndex = 1;
            string[] SimilarityMethods = new string[] { "Distance 1D", "Distance 3D", "Ratio", "Ratio+" };
            for (int i = 0; i < SimilarityMethods.Length; i++)
            {
                comboBox2.Items.Add(SimilarityMethods[i]);
            }
            comboBox2.SelectedIndex = 0;
            da.SongAddedToDBEvent += new EventHandler<SongAddedEventArgs>(SongAdded);
            ConfigureDBTab();
            border1.Height = 0;
            label3.FontWeight = FontWeights.Bold;
            FilesAddTerminared = false;
            label2.Content = "Shows processed files";
        }

        private void ConfigureDBTab()
        {
            CurrentProgress = 0;
            progressBar1.Value = 0;
            button1.IsEnabled = true;
            button3.IsEnabled = true;
            button4.IsEnabled = false;
        }   

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int DegreeOfParallelism = 1;
            listBox2.Items.Clear();
            button1.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = true;
            label2.Content = "Searching for music files";
            tabControl1.IsEnabled = false;
            if (checkBox1.IsChecked==true)
                DegreeOfParallelism = Environment.ProcessorCount;
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                var FileList = Directory.GetFiles(fbd.SelectedPath, "*.mp3", SearchOption.AllDirectories);
                if (FileList.Length > 0)
                {
                    CollectionToProcessSize = FileList.Length;
                    progressBar1.Maximum = CollectionToProcessSize;
                    Task t = new Task(() => da.ProcessManySongsParallel(FileList, DegreeOfParallelism));
                    t.ContinueWith((o) =>
                    {
                        da.RemoveDuplicatesFromDB();
                        dt = da.GetDataSet().Tables[0];
                        dt_temp = da.GetDataSet().Tables[0];
                        Dispatcher.Invoke((Action)(() => dataGrid1.ItemsSource = dt_temp.DefaultView));
                        if (FilesAddTerminared)
                            Dispatcher.Invoke((Action)(() => label2.Content = "Operation was terminated by user"));
                        else
                            Dispatcher.Invoke((Action)(() => label2.Content = CollectionToProcessSize + " titles were processed"));
                        Dispatcher.Invoke((Action)(() => label3.Content = "Your music library contains " + dt.Rows.Count + " titles"));
                        FilesAddTerminared = false;
                        Dispatcher.Invoke((Action)(() => ConfigureDBTab()));
                    });
                    t.Start();
                }
                else
                {
                    label2.Content = "There is now music files in the specified folder";
                }
            }
            else
            { 
                ConfigureDBTab(); 
            }
                 
            tabControl1.IsEnabled = true;
        }

        private void SongAdded(object sender, SongAddedEventArgs e)
        {
            CurrentProgress++;
            Dispatcher.Invoke((Action)(() => 
            {
                listBox2.Items.Add(e.SongName);
                label2.Content = "Processed " + CurrentProgress.ToString() + " of " + CollectionToProcessSize.ToString();
                progressBar1.Value = CurrentProgress;
                listBox2.ScrollIntoView(listBox2.Items[listBox2.Items.Count-1]);
            }));
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (da.ClearDB())
            {
                tabControl1.IsEnabled = false;
                dt = da.GetDataSet().Tables[0];
                dt_temp = da.GetDataSet().Tables[0];
                dataGrid1.ItemsSource = da.GetDataSet().Tables[0].DefaultView;
                label3.Content = "Your music library is empty now";
                MessageBox.Show("Your DataBase is empty!");
                tabControl1.IsEnabled = true;
            }
        }

        private void dataGrid1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "song_id")
            {
                e.Cancel = true;
            }
            if (e.Column.Header.ToString() == "path")
            {
                e.Cancel = true;
            }
            if (e.Column.Header.ToString() == "time")
            {
                e.Column.Width = 50;
            }
        }


        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            string pattern = textBox1.Text;
            DataRow[] dr = dt.Select("title like '%" + pattern + "%' or artist like '%" + pattern + "%'");
            DataTable temp=new DataTable();
            if (dr.Length > 0)
                temp = dr.CopyToDataTable();
            dt_temp = temp;
            dataGrid1.ItemsSource = dt_temp.DefaultView;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = dataGrid1.SelectedItem as DataRowView;
            if (drv != null)
            {
                tabControl1.IsEnabled = false;
                try
                {
                    System.Diagnostics.Process.Start(drv.Row.ItemArray[4].ToString());
                }
                catch (FileNotFoundException Ex)
                {
                    MessageBox.Show("File not found. It is probably deleted or moved to another location.");
                }
                finally
                {
                    tabControl1.IsEnabled = true;
                }
            }  
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            int[] ind;
            DataRowView drv = dataGrid1.SelectedItem as DataRowView;
            if (drv != null)
            {
                tabControl1.IsEnabled = false;
                ind = da.GetSimilarSongs(Convert.ToInt32(drv.Row.ItemArray[0]),comboBox2.SelectedIndex+1,(int)comboBox1.SelectedValue);
                DataTable TempTable = new DataTable();
                DataRow[] TempRows=new DataRow[ind.Length];
                for (int k = 0; k < ind.Length; k++)
                {
                    DataRow[] dr = dt.Select("song_id=" + ind[k].ToString());
                    TempRows[k] = dr[0];
                }
                TempTable = TempRows.CopyToDataTable();
                ShowSongsWindow SimilarSongWindow = new ShowSongsWindow();
                SimilarSongWindow.Title = drv.Row.ItemArray[2].ToString() + " - " + drv.Row.ItemArray[3].ToString();
                SimilarSongWindow.dataGrid1.DataContext = TempTable;
                SimilarSongWindow.data = TempTable;
                SimilarSongWindow.Owner = this;
                SimilarSongWindow.Show();
                tabControl1.IsEnabled = true;
            }
        }


        private void button4_Click(object sender, RoutedEventArgs e)
        {
            FilesAddTerminared = true;
            da.StopProcessing();
            button4.IsEnabled = false;
            label3.Content = "Your music library contains " + dt.Rows.Count + " titles";
        }


        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView drv = dataGrid1.SelectedItem as DataRowView;
            if (drv != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(drv.Row.ItemArray[4].ToString());
                }
                catch (FileNotFoundException Ex)
                {
                    MessageBox.Show("File not found. It is probably deleted or moved to another location.");
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            DataRowView drv = dataGrid1.SelectedItem as DataRowView;
            if (drv != null)
            {
                tabControl1.IsEnabled = false;
                string args = string.Format("/Select, {0}", drv.Row.ItemArray[4].ToString());
                ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
                try
                {

                    System.Diagnostics.Process.Start(pfi);
                }
                catch (FileNotFoundException Ex)
                {
                    MessageBox.Show("File not found. It is probably deleted or moved to another location.");
                }
                finally
                {
                    tabControl1.IsEnabled = true;
                }
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.IsEnabled = false;
            label3.Content = "Please wait...";
            da.SaveDataSet(dt_temp);
            dt = da.GetDataSet().Tables[0];
            MessageBox.Show("Your Library is updated!");
            label3.Content = "Your music library contains " + dt.Rows.Count + " titles";
            tabControl1.IsEnabled = true;
        }


        #region CheckboxStates
        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            radioButton1.IsEnabled = true;
            radioButton2.IsEnabled = true;
        }

        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            radioButton1.IsEnabled = false;
            radioButton2.IsEnabled = false;
        }

        private void checkBox3_Checked(object sender, RoutedEventArgs e)
        {
            radioButton3.IsEnabled = true;
            radioButton4.IsEnabled = true;
        }

        private void checkBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            radioButton3.IsEnabled = false;
            radioButton4.IsEnabled = false;
        }

        private void checkBox4_Checked(object sender, RoutedEventArgs e)
        {
            radioButton5.IsEnabled = true;
            radioButton6.IsEnabled = true;
        }

        private void checkBox4_Unchecked(object sender, RoutedEventArgs e)
        {
            radioButton5.IsEnabled = false;
            radioButton6.IsEnabled = false;
        }

        private void checkBox5_Checked(object sender, RoutedEventArgs e)
        {
            radioButton7.IsEnabled = true;
            radioButton8.IsEnabled = true;
        }

        private void checkBox5_Unchecked(object sender, RoutedEventArgs e)
        {
            radioButton7.IsEnabled = false;
            radioButton8.IsEnabled = false;
        }

        private void checkBox6_Checked(object sender, RoutedEventArgs e)
        {
            radioButton9.IsEnabled = true;
            radioButton10.IsEnabled = true;
        }

        private void checkBox6_Unchecked(object sender, RoutedEventArgs e)
        {
            radioButton9.IsEnabled = false;
            radioButton10.IsEnabled = false;
        }
        #endregion

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.IsEnabled = false;
            FrequencyState[] state = new FrequencyState[5]; 
            string SearchCondition = "";
            #region CheckBoxState
            if (checkBox2.IsChecked==true)
                if (radioButton1.IsChecked == true)
                {
                    state[0] = FrequencyState.Min;
                    SearchCondition+="SubBass-Min";
                }
                else
                {
                    state[0] = FrequencyState.Max;
                    SearchCondition += "SubBass-Max ";
                }
            if (checkBox3.IsChecked == true)
                if (radioButton3.IsChecked == true)
                {
                    state[1] = FrequencyState.Min;
                    SearchCondition += "Bass-Min ";
                }
                else
                {
                    state[1] = FrequencyState.Max;
                    SearchCondition += "Bass-Max ";
                }
            if (checkBox4.IsChecked == true)
                if (radioButton5.IsChecked == true)
                {
                    state[2] = FrequencyState.Min;
                    SearchCondition += "Mid-Min ";
                }
                else
                {
                    state[2] = FrequencyState.Max;
                    SearchCondition += "Mid-Max ";
                }
            if (checkBox5.IsChecked == true)
                if (radioButton7.IsChecked == true)
                {
                    state[3] = FrequencyState.Min;
                    SearchCondition += "HighMid-Min ";
                }
                else
                {
                    state[3] = FrequencyState.Max;
                    SearchCondition += "HighMid-Max ";
                }
            if (checkBox6.IsChecked == true)
                if (radioButton9.IsChecked == true)
                {
                    state[4] = FrequencyState.Min;
                    SearchCondition += "High-Min ";
                }
                else
                {
                    state[4] = FrequencyState.Max;
                    SearchCondition += "High-Max ";
                }
            #endregion
            int[] ind=da.GetSongsBySpectra(state, (int)comboBox1.SelectedValue);
            DataTable TempTable = new DataTable();
            DataRow[] TempRows = new DataRow[ind.Length];
            for (int k = 0; k < ind.Length; k++)
            {  
                DataRow[] dr = dt.Select("song_id=" + ind[k].ToString());
                TempRows[k] = dr[0];
            }
            TempTable = TempRows.CopyToDataTable();
            ShowSongsWindow SpectraSongWindow = new ShowSongsWindow();
            SpectraSongWindow.ToolTip = "Double-click on item to play";
            SpectraSongWindow.Title = SearchCondition;
            SpectraSongWindow.dataGrid1.DataContext = TempTable;
            SpectraSongWindow.data = TempTable;
            SpectraSongWindow.Owner = this;
            SpectraSongWindow.Show();
            tabControl1.IsEnabled = true;
        }


        private void checkBox7_Checked(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = 223;
            animation.Duration = TimeSpan.FromSeconds(0.5);
            border1.BeginAnimation(Border.HeightProperty, animation);
        }

        private void checkBox7_Unchecked(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = 0;
            animation.Duration = TimeSpan.FromSeconds(0.5);
            border1.BeginAnimation(Border.HeightProperty, animation);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!da.IsSafeToQuit)
            {
                string msg = "Music files are being processed now." +Environment.NewLine+ "Please wait for the operation completes or stop the process";
                MessageBox.Show(msg, "App Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
            }
        }

    }
}
