using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

namespace frontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("../../../../CppClassDll/Debug/CppClassDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void openStl(string fname);

        private void openFileClick(object sender, RoutedEventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Open model";
            ofd.Filter = "STL files|*.STL";
            if (ofd.ShowDialog() ==true)
            {
                string filename = ofd.FileName.Replace("\\", "/");
                openStl(filename);
            }
        }
        private void newPathClick(object sender, RoutedEventArgs e)
        {
            if (canvas.IsEnabled==false)
            {
                pathCoords.Items.Clear();
                polyline.Points.Clear();
                canvas.IsEnabled = true;
                new_path.Content = "End Path";
            }
            else
            {
                canvas.IsEnabled = false;
                new_path.Content = "New Path";
            }
        }
        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            double x = p.X;
            double y = p.Y;
            polyline.Points.Add(p);
            pathCoords.Items.Add("X: " + x.ToString() + "   y: " + y.ToString());
        }


    }
}
