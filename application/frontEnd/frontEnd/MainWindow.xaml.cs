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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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
        private List<Point3D> path = new List<Point3D>();
        private validationObject valObj;
        private readSTL model;
        private String folderPath;
        private int filenum;

        private void openFileClick(object sender, RoutedEventArgs e)
        {
            
            
            // Create an instance of the open file dialog box.
            var ofd = new OpenFileDialog();

            ofd.Title = "Open model";
            ofd.Filter = "STL files|*.STL";
            if (ofd.ShowDialog() ==true)
            {
                string filename = ofd.FileName.Replace("\\", "/");
                model = new readSTL(filename);
            }
            if (path.Count != 0 && model != null) //enable simulation button when necessary data acquired
            {
                runJob.IsEnabled = true;
            }

        }

        //open text file with path
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
            if (path.Count != 0 && model != null) //enable simulation button when necessary data acquired
            {
                runJob.IsEnabled = true;
            }
            /* 
            //draw path on canvas test
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
        //read text file with path and store path in path list
        private void readPath(string pathName)
        {
            string[] lines = System.IO.File.ReadAllLines(pathName);
            int nodeNums = Int32.Parse(lines[4].Split(' ')[1]);

            // Display the file contents by using a foreach loop.
            for (var i = 5; i < 5 + Math.Ceiling((double)nodeNums / 3.0) ; i++)
            {
                
                String[] coords = lines[i].Trim().Split(' ');
                for (var j=0; j<coords.Length-2; j+=3)
                {
                    Point3D newNode = new Point3D();
                    newNode = Point3D.Parse(coords[j] + "," + coords[j + 1] + "," + coords[j + 2]);
                    path.Add(newNode);
                    pathCoords.Items.Add(coords[j]+ " " + coords[j+1] + " " + coords[j + 2]);
                }
            }
        }

        //NOT USED
        //get position from clicking on canvas testing
        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            double x = p.X;
            double y = p.Y;
            polyline.Points.Add(p);
            pathCoords.Items.Add("X: " + x.ToString() + "   y: " + y.ToString());
        }

        //iterate through path of nodes
        private Boolean iteratePath(List<Point3D> path)
        {
            valObj = new validationObject(1000, 2000, 700, 5, path[0], path[1]);
            filenum = 0;
            writeTrolleyFile(filenum, false);
            filenum++;
            for (var i=1; i<path.Count; i++)
            {
                
                //display of test data
                //testData.Items.Add("previousNode: " + valObj.currentPosition.X + ", " + valObj.currentPosition.Y + ", " + valObj.currentPosition.Z);
                //testData.Items.Add("nextNode: " + path[i].X + ", " + path[i].Y + ", " + path[i].Z);
                valObj.rotateTrolley(path[i]);
                //testData.Items.Add("YOLOYOLO NEW ANGLE IS: X: " + valObj.nextAngleYZ + " Y: "+ valObj.nextAngleXZ + " Z: " + valObj.nextAngleXY);
                //run dynamic relaxation
                if (dynamicRelaxation(path[i])) { 
                    testData.Items.Add("dynamic relaxation done for this node");
                    continue;
                }
                else
                    return false;
                
            }
            
            return true;
        }
            
        private Boolean dynamicRelaxation(Point3D nextNode)
        {
            var deltaT = 0.8; //0.1 during clash

            var residualForce = new Vector3D(0.0, 0.0, 0.0);
            var clashForces = new Vector3D(0.0, 0.0, 0.0); // External force
            var internalForce = new Vector3D(0.0, 0.0, 0.0); //
            var dampingForce = new Vector3D(0.0, 0.0, 0.0); //

            var displacement = new Vector3D(0.0, 0.0, 0.0);
            var velocity = new Vector3D(0.0, 0.0, 0.0);
            var acceleration = new Vector3D(0.0, 0.0, 0.0);

            //set displacement between current position and next path position
            displacement = valObj.currentPosition - nextNode; //OBS CURRENTPOSITION gir en liten endring i displacement ved 0 crash

            var first = true;

            while (first==true || residualForce.Length> 1.0e-5) {
                first=false; //check
                if (clashForce())
                {
                    writeTrolleyFile(filenum, true);
                    filenum++;
                }
                internalForce = valObj.K * displacement;
                dampingForce  = valObj.C * velocity;

                residualForce = clashForces - internalForce - dampingForce;

                //new acceleration, velocity and the new change of displacement
                acceleration = residualForce / valObj.mass;
                velocity     += acceleration*deltaT;
                displacement += velocity * deltaT;

                valObj.updateObjectPosition(velocity * deltaT);
                
                //just to check out the data, should be removed when works fully
                //testData.Items.Add("D: " + displacement.Length + "      R: " + residualForce.Length + "      V: " + velocity.Length);
                
            }

            valObj.newPath.Add(valObj.currentPosition); //change node to the actuall new position from displacement
            writeTrolleyFile(filenum, false);
            filenum++;
            return true;
        }
        private Boolean boundingBoxCheck(List<Vector3D> tri)
        {
            var sortX = valObj.trolley.OrderBy(point => point.X);
            var sortY = valObj.trolley.OrderBy(point => point.Y);
            var sortZ = valObj.trolley.OrderBy(point => point.Z);

            return tri[1].X > sortX.Last().X && tri[2].X > sortX.Last().X && tri[3].X > sortX.Last().X
                    || tri[1].X < sortX.First().X && tri[2].X < sortX.First().X && tri[3].X < sortX.First().X
                    || tri[1].Y > sortY.Last().Y && tri[2].Y > sortY.Last().Y && tri[3].Y > sortY.Last().Y
                    || tri[1].Y < sortY.First().Y && tri[2].Y < sortY.First().Y && tri[3].Y < sortY.First().Y
                    || tri[1].Z > sortZ.Last().Z && tri[2].Z > sortZ.Last().Z && tri[3].Z > sortZ.Last().Z
                    || tri[1].Z < sortZ.First().Z && tri[2].Z < sortZ.First().Z && tri[3].Z < sortZ.First().Z;
        }
        
        private bool clashForce()
        {
            //Vector3D clashForce = new Vector3D();

            int numberOfCrashes = 0;
            bool crash = false;
            foreach(List<Vector3D> tri in model.boundary)
            {
                if(boundingBoxCheck(tri))
                {
                    continue;
                }
                else
                {
                    foreach(var line in valObj.linePoints)
                    {
                        if (Vector3D.DotProduct(tri[0], (valObj.trolley[line[0]] - valObj.trolley[line[1]])) != 0)
                        {
                            var A = Matrix<double>.Build.DenseOfArray(new[,] {
                                {valObj.trolley[line[0]].X - valObj.trolley[line[1]].X, tri[2].X-tri[1].X,tri[3].X-tri[1].X},
                                {valObj.trolley[line[0]].Y - valObj.trolley[line[1]].Y, tri[2].Y-tri[1].Y,tri[3].Y-tri[1].Y},
                                {valObj.trolley[line[0]].Z - valObj.trolley[line[1]].Z, tri[2].Z-tri[1].Z,tri[3].Z-tri[1].Z}});

                            var B = Matrix<double>.Build.DenseOfArray(new[,] {
                                {valObj.trolley[line[0]].X - tri[1].X},
                                {valObj.trolley[line[0]].Y - tri[1].Y},
                                {valObj.trolley[line[0]].X - tri[1].Z}});
                            var inter = A.Inverse()*B;
                            if (0 < inter[0, 0] && inter[0,0] < 1 && 0 < inter[1, 0] && inter[1, 0] < 1 &&
                                0 < inter[2, 0] && inter[2, 0] < 1 && inter[1, 0] + inter[2, 0] <= 1)
                            {
                                numberOfCrashes++;
                                crash = true;
                            }
                        }
                    }
                        
                }
            }
            testData.Items.Add(numberOfCrashes);
            return crash;
        }

        // action of simulation button
        private void runJob_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Folder location";
            saveFileDialog1.ShowDialog();
            canvas.IsEnabled = true;
            if (saveFileDialog1.FileName != "")
            {
                folderPath = saveFileDialog1.FileName;
                iteratePath(path);
            }
        }
        private void writeTrolleyFile(int i, bool clash)
        {
            String[] startText = { "# vtk DataFile Version 4.0", "vtk output", "ASCII", "DATASET POLYDATA", "POINTS 8 float" };
            String[] endText = { "POLYGONS 6 30", "4 0 1 3 2 4 2 3 5 4 4 4 5 7 6 4 0 1 7 6 4 0 2 4 6 4 1 3 5 7" };
            String green = "0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0";
            String red = "1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0";
            String[] color = { "CELL_DATA 6", "SCALARS cell_scalars int 1", "LOOKUP_TABLE default", "0 1 2 3 4 5", "LOOKUP_TABLE default 6" };
            
            System.IO.Directory.CreateDirectory(folderPath);
            using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(folderPath + @"\jobb" + i.ToString() + ".vtk"))
            {
                foreach (var line in startText)
                {
                    file.WriteLine(line);

                }
                foreach (var trolleyPoint in valObj.trolley)
                {
                    file.WriteLine(trolleyPoint.X.ToString().Replace(",", "."));
                    file.WriteLine(trolleyPoint.Y.ToString().Replace(",", "."));
                    file.WriteLine(trolleyPoint.Z.ToString().Replace(",", "."));
                }
                foreach (var line in endText)
                {
                    file.WriteLine(line);

                }
                foreach (var line in color)
                {
                    file.WriteLine(line);

                }
                if (clash)
                {
                    file.WriteLine(red);
                }
                else
                {
                    file.WriteLine(green);
                }
        }
        }
    }
}

