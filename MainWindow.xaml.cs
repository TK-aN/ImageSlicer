using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace ImageSlicer
{

    public partial class MainWindow : Window
    {
        BitmapImage image;
        Bitmap bitmap;

        int sliceMode = 0; //0:Pixel 1:SliceCount
        int valueX = 0, valueY = 0;
        int countX = 0, countY = 0;

        string outputPath;
        AssemblyName assembly = Assembly.GetExecutingAssembly().GetName();

        public MainWindow()
        {
            InitializeComponent();
            this.Title = assembly.Name + " " + assembly.Version;
        }

        private void button_Click_Run(object sender, RoutedEventArgs e)
        {
            if (bitmap == null) return;

            outputPath = System.IO.Path.GetDirectoryName(textBox_InputPath.Text) + @"\" + System.IO.Path.GetFileNameWithoutExtension(textBox_InputPath.Text);

            if (System.IO.Directory.Exists(outputPath))
            {
                outputPath += "_" + DateTime.Now.ToString("MMddHHmmss");
                
                while (System.IO.Directory.Exists(outputPath))
                {
                    outputPath += "_";
                }

                Directory.CreateDirectory(outputPath);
            }
            else
            {
                Directory.CreateDirectory(outputPath);
            }

            text_Line2.Text = "<OutputPath> " + outputPath;

            int bmWidth = valueX;
            int bmHeight = valueY;

            for (int i_x = 0; i_x < countX; i_x++)
            {
                for(int i_y = 0; i_y < countY; i_y++)
                {
                    Bitmap bm = new Bitmap(bmWidth, bmHeight);

                    for (int j_x = 0; j_x < bmWidth; j_x++)
                    {
                        for(int j_y = 0; j_y < bmHeight; j_y++)
                        {
                            int x = j_x + i_x * bmWidth;
                            int y = j_y + i_y * bmHeight;

                            if(x >= bitmap.Width || y >= bitmap.Height)
                                bm.SetPixel(j_x, j_y, System.Drawing.Color.Transparent);
                            else
                                bm.SetPixel(j_x, j_y, bitmap.GetPixel(x, y));

                        }
                    }

                    bm.Save(outputPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(textBox_InputPath.Text) + "_" + (i_x + 1)  + "_" + (i_y + 1) + ".png", ImageFormat.Png);
                    bm.Dispose();
                }
            }

            MessageBox.Show("出力完了", assembly.Name, MessageBoxButton.OK);
            System.Media.SystemSounds.Asterisk.Play();
        }



        private void button_Click_Browse(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "image files (*.png;*.jpg)|*.png;*jpg";

            if (dialog.ShowDialog() == true)
            {
                textBox_InputPath.Text = dialog.FileName;

                setImage(dialog.FileName);
            }
        }

        private void numPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }


        private void sliceMode_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == sliceMode0) sliceMode = 0;
            else if (radioButton == sliceMode1) sliceMode = 1;
        }

        private void valueChanged(object sender, EventArgs e)
        {
            updateUnitSize();
        }

        private void checkBox_Click_value(object sender, RoutedEventArgs e)
        {
            if (checkBox_valueX.IsChecked == true) textBox_valueX.IsEnabled = true;
            else textBox_valueX.IsEnabled = false;

            if (checkBox_valueY.IsChecked == true) textBox_valueY.IsEnabled = true;
            else textBox_valueY.IsEnabled = false;

            updateUnitSize();
        }


        private void setImage(string path)
        {
            var stream = File.OpenRead(@path);

            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();

            bitmap = new Bitmap(stream);

            stream.Close();
            stream.Dispose();

            imagePreview.Source = image;
            text_ImageSize.Text = "<ImageSize> " + image.PixelWidth.ToString() + "x" + image.PixelHeight.ToString();

            updateUnitSize();
        }

       

        private void updateUnitSize()
        {
            if (bitmap == null) return;

            if (textBox_valueX.Text.Equals("")) valueX = 0;
            else if (checkBox_valueX.IsChecked == true) valueX = int.Parse(textBox_valueX.Text);
            else valueX = 0; 

            if (textBox_valueY.Text.Equals("")) valueY = 0;
            else if (checkBox_valueY.IsChecked == true ) valueY = int.Parse(textBox_valueY.Text);
            else valueY = 0;

            if (sliceMode == 0) //Pixel基準 操作なし
            {
                //
            }

            if (sliceMode == 1) //SliceCountをPixelに変換する.
            {
                if (valueX > 0) valueX = (bitmap.Width + valueX - 1) / valueX;
                if (valueY > 0) valueY = (int)Math.Ceiling((double)bitmap.Height / valueY);
            }

            if (valueX == 0) valueX = bitmap.Width;
            if (valueY == 0) valueY = bitmap.Height;

            countX = (bitmap.Width + valueX - 1) / valueX;
            countY = (bitmap.Height + valueY - 1) / valueY;

            text_Line1.Text = "<UnitSize> " + valueX.ToString() + " x " + valueY.ToString() + " (" + countX + " x " + countY + ")";

        }
    }
}
