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
using System.IO;

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
        private readPath path;
        private validationObject valObj;
        private readSTL model;
        private String folderPath;
        private int filenum;
        private bool crashing = false;
        bool crashingcheck = false;
        List<List<Vector3D>> testingOfCrashedWalls=new List<List<Vector3D>>();

        private void openFileClick(object sender, RoutedEventArgs e)
        {
            
            
            // Create an instance of the open file dialog box.
            var ofd = new OpenFileDialog();

            ofd.Title = "Open model";
            ofd.Filter = "STL files|*.STL";
            if (ofd.ShowDialog() ==true)
            {
                model = new readSTL(ofd.FileName.Replace("\\", "/"));
            }
            if (path != null && model != null) //enable simulation button when necessary data acquired
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
                path = new readPath(ofd.FileName.Replace("\\", "/"));
                pathCoords.Items.Clear();
                foreach(Point3D node in path.path)
                {
                    pathCoords.Items.Add(node.X + " " + node.Y + " " + node.Z);
                }
            }
            if (path != null && model != null) //enable simulation button when necessary data acquired
            {
                runJob.IsEnabled = true;
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
        private Boolean iteratePath()
        {
            filenum = 0;
            writeTrolleyFile(filenum, false);
            filenum++;
            for (var i=1; i<path.path.Count; i++)
            {              
                valObj.rotateTrolley(path.path[i]);

                if (dynamicRelaxation(path.path[i])) { 
                    testData.Items.Add("dynamic relaxation done for node: " +i);
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
                crashingcheck = clashForce();
                if (crashingcheck!=crashing)
                {
                    writeTrolleyFile(filenum, crashingcheck);
                    filenum++;
                    crashing = crashingcheck;
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
            writeTrolleyFile(filenum, crashingcheck);
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
            foreach (List<Vector3D> tri in model.boundary)
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
                            var t1 = Matrix<double>.Build.DenseOfArray(new[,] {
                                {1, 1, 1, 1 },
                                {tri[1].X, tri[2].X, tri[3].X, valObj.trolley[line[1]].X},
                                {tri[1].Y, tri[2].Y, tri[3].Y, valObj.trolley[line[1]].Y},
                                {tri[1].Z, tri[2].Z, tri[3].Z, valObj.trolley[line[1]].Z}});
                            var t2 = Matrix<double>.Build.DenseOfArray(new[,] {
                                {1, 1, 1, 0 },
                                {tri[1].X, tri[2].X, tri[3].X, valObj.trolley[line[0]].X-valObj.trolley[line[1]].X},
                                {tri[1].Y, tri[2].Y, tri[3].Y, valObj.trolley[line[0]].Y-valObj.trolley[line[1]].Y},
                                {tri[1].Z, tri[2].Z, tri[3].Z, valObj.trolley[line[0]].Z-valObj.trolley[line[1]].Z}});
                            var t = -t1.Determinant() / t2.Determinant();                        

                            if (0<t && t<1)
                            {
                                Vector3D intersect = new Vector3D(valObj.trolley[line[1]].X + ((valObj.trolley[line[0]].X) - valObj.trolley[line[1]].X) * t, valObj.trolley[line[1]].Y + ((valObj.trolley[line[0]].Y) - valObj.trolley[line[1]].Y) * t, valObj.trolley[line[1]].Z + ((valObj.trolley[line[0]].Z) - valObj.trolley[line[1]].Z) * t);
                                if (insideTriangle(intersect, tri))
                                {
                                    if (!testingOfCrashedWalls.Contains(tri))
                                    {
                                        testingOfCrashedWalls.Add(tri);
                                    }
                                    numberOfCrashes++;
                                    crash = true;
                                }
                            }
                        }
                    }
                        
                }
            }
            if(numberOfCrashes!=0)
                testData.Items.Add(numberOfCrashes);
            testData.Items.Add("number of walls crashed in until now: " + testingOfCrashedWalls.Count);
            return crash;
        }
        private bool insideTriangle(Vector3D intersect, List<Vector3D> tri)
        {
            Vector3D v0 = tri[3] - tri[1];
            Vector3D v1 = tri[2] - tri[1];
            Vector3D v2 = intersect - tri[1];

            double dot00 = Vector3D.DotProduct(v0, v0);
            double dot01 = Vector3D.DotProduct(v0, v1);
            double dot02 = Vector3D.DotProduct(v0, v2);
            double dot11 = Vector3D.DotProduct(v1, v1);
            double dot12 = Vector3D.DotProduct(v1, v2);

            double invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            return (u >= 0) && (v >= 0) && (u + v < 1);
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
                valObj = new validationObject(double.Parse(length.Text), double.Parse(width.Text), double.Parse(height.Text), path.path[0], path.path[1]);
                iteratePath();
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

