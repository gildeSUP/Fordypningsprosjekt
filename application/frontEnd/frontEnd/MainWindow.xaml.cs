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
using System.Windows.Media.Media3D;

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

        //obsobs: er i millimeter
        List<Point3D> path = new List<Point3D> { new Point3D ( 1559.03, 160.589, 1274.49 ), new Point3D(1408.93, 8102.45, 1274.49), new Point3D(2001.82, 9093.03, 1274.49 ), new Point3D( 2742.6, 9614.62, 1274.49 ) };
        List<Point3D> newPath = new List<Point3D>();

       [DllImport("../../../../CppClassDll/Debug/CppClassDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void openStl(string fname);

        private void openFileClick(object sender, RoutedEventArgs e)
        {
            iteratePath(path);
            /*
            // Create an instance of the open file dialog box.
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Open model";
            ofd.Filter = "STL files|*.STL";
            if (ofd.ShowDialog() ==true)
            {
                string filename = ofd.FileName.Replace("\\", "/");
                openStl(filename);
            }
            */
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

        private Boolean iteratePath(List<Point3D> path)
        {
            
            newPath.Add(path[0]);
            foreach (Point3D node in path)
            {
                pathCoords.Items.Add(node.X + ", " + node.Y + ", " + node.Z);
                if (dynamicRelaxation(node)) { 
                    pathCoords.Items.Add("ok");
                    continue;
                }
                else
                    return false;
                
            }
            return true;
        }
        private Boolean dynamicRelaxation(Point3D node)
        {
            double alpha = 0.1;
            Vector3D residualForce = new Vector3D(0.0, 0.0, 0.0);
            Vector3D clashForce = new Vector3D(0.0, 0.0, 0.0);

            Vector3D displacement = new Vector3D(0.0, 0.0, 0.0);
            Vector3D velocity = new Vector3D(0.0, 0.0, 0.0);
            Vector3D acceleration = new Vector3D(0.0, 0.0, 0.0);

            double KObject = 0.3;
            double CObject = 0.6; //2*Math.sqrt(m*sum(K))
            
            double mass = 2;
            double deltaT = 0.8; //0.1 during clash
            Point3D N = newPath.Last(); //newPath[i]
            displacement.X = N.X - node.X;
            displacement.Y = N.Y - node.Y;
            displacement.Z = N.Z - node.Z;
            Boolean first = true;

            while (first==true || residualForce.Length>10) {
                first=false;

                residualForce = residualForce*alpha+(1.0-alpha)*(clashForce-KObject*displacement-CObject*velocity);

                acceleration = residualForce/mass;
                velocity += acceleration*deltaT;
                displacement += velocity * deltaT;

                pathCoords.Items.Add("D: " + displacement.Length);
                pathCoords.Items.Add("R: " + residualForce.Length);
                
            }
            
            newPath.Add(N);
            return true;
        }

    }
}

