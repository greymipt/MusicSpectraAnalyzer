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
using System.Windows.Shapes;
using System.Data;
using System.IO;
using System.Diagnostics;
using winforms=System.Windows.Forms;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for ShowSongsWindow.xaml
    /// </summary>
    public partial class ShowSongsWindow : Window
    {
        public ShowSongsWindow()
        {
            InitializeComponent();
        }

        public DataTable data {get; set;}

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
                e.Cancel = true;
            }
        }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView drv = dataGrid1.SelectedItem as DataRowView;
            if (drv != null)
            {
                System.Diagnostics.Process.Start(drv.Row.ItemArray[4].ToString());
            }  
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("Playlists");
            winforms.SaveFileDialog saveFileDialog1 = new winforms.SaveFileDialog();
            saveFileDialog1.Filter = "m3u files (*.m3u)|*.m3u*";
            saveFileDialog1.DefaultExt = "m3u";
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory()+"\\Playlists";
            if (saveFileDialog1.ShowDialog() == winforms.DialogResult.OK)
            {
                string FileName = saveFileDialog1.FileName;
                using (FileStream fs = new FileStream(FileName, FileMode.Create))
                {
                    Encoding myEncoding = Encoding.Default;
                    using (StreamWriter writer = new StreamWriter(fs, myEncoding))
                    {
                        for (int i = 0; i < data.Rows.Count; i++)
                        {
                            writer.WriteLine(data.Rows[i].ItemArray[4].ToString());
                        }  
                    }
                }
            }    
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string FileName="temp.m3u";
            using (FileStream fs = new FileStream(FileName, FileMode.Create))
            {
                Encoding myEncoding = Encoding.Default;
                using (StreamWriter writer = new StreamWriter(fs, myEncoding))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        writer.WriteLine(data.Rows[i].ItemArray[4].ToString());
                    }
                }
            }
            System.Diagnostics.Process.Start(FileName);    
        }
    }
}
