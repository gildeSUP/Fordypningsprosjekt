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
        //List<Point3D> path = new List<Point3D> { new Point3D ( 1559.03, 160.589, 1274.49 ), new Point3D(1408.93, 8102.45, 1274.49), new Point3D(2001.82, 9093.03, 1274.49 ), new Point3D( 2742.6, 9614.62, 1274.49 ) };
        List<Point3D> path = new List<Point3D>();
        List<Point3D> newPath = new List<Point3D>();

       [DllImport("../../../../CppClassDll/Debug/CppClassDll.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void openStl(string fname);

        private void openFileClick(object sender, RoutedEventArgs e)
        {
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
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Choose Path";
            ofd.Filter = "VTK-files|*.vtk";
            if (ofd.ShowDialog() == true)
            {
                string pathName = ofd.FileName.Replace("\\", "/");
                readPath(pathName);
            }
            if (path.Count != 0)
            {
                runJob.IsEnabled = true;
            }
            /*
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
            }'*/
        }

        private void readPath(string pathName)
        {
            string[] lines = System.IO.File.ReadAllLines(pathName);
            int nodeNums = Int32.Parse(lines[4].Split(' ')[1]);

            // Display the file contents by using a foreach loop.
            for (int i = 5; i < 5 + Math.Ceiling((double)nodeNums / 3.0) ; i++)
            {
                
                String[] coords = lines[i].Trim().Split(' ');
                for (int j=0; j<coords.Length-2; j+=3)
                {
                    Point3D newNode = new Point3D();
                    newNode = Point3D.Parse(coords[j] + "," + coords[j + 1] + "," + coords[j + 2]);
                    path.Add(newNode);
                    pathCoords.Items.Add(coords[j]+ " " + coords[j+1] + " " + coords[j + 2]);
                }
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
                testData.Items.Add("previousNode: " + newPath.Last().X + ", " + newPath.Last().Y + ", " + newPath.Last().Z);
                testData.Items.Add("nextNode: " + node.X + ", " + node.Y + ", " + node.Z);
                if (dynamicRelaxation(node)) { 
                    testData.Items.Add("dynamic relaxation done for this node");
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

            double KObject = 0.05;
            double CObject = 0.1; //2*Math.sqrt(m*sum(K))
            
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

                testData.Items.Add("D: " + displacement.Length + "      R: " + residualForce.Length + "      V: " + velocity.Length);
                
            }
            
            newPath.Add(N);
            return true;
        }

        private void runJob_Click(object sender, RoutedEventArgs e)
        {
            canvas.IsEnabled = true;
            iteratePath(path);
        }
    }
}

