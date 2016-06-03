using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra;
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

        //OBS: Alt gjøres i millimeter
        //List<Point3D> path = new List<Point3D> { new Point3D ( 1559.03, 160.589, 1274.49 ), new Point3D(1408.93, 8102.45, 1274.49), new Point3D(2001.82, 9093.03, 1274.49 ), new Point3D( 2742.6, 9614.62, 1274.49 ) };
        private readPath path;
        private validationObject valObj;
        private readSTL model;
        private String folderPath;
        private bool crashing = false;
        bool crashingCheck = false;
        List<List<Vector3D>> testingOfCrashedWalls=new List<List<Vector3D>>();
        private bool validate;
        private double kWall = 0.008;

        //open STL-file with model
        private void openFileClick(object sender, RoutedEventArgs e)
        {
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

        //open VTK-file with path
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

        // action of simulation button
        private void runJob_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox1.IsChecked == true || checkBox2.IsChecked == true)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Title = "Folder location";
                saveFileDialog1.ShowDialog();
                canvas.IsEnabled = true;
                if (saveFileDialog1.FileName != "")
                {
                    folderPath = saveFileDialog1.FileName;
                    if (checkBox1.IsChecked == true)
                    {
                        valObj = new validationObject(double.Parse(length.Text), double.Parse(width.Text), double.Parse(height.Text), path.path[0], path.path[1]);
                        validate = true;
                        iteratePath();
                        System.IO.Directory.CreateDirectory(folderPath);
                        writeFiles(valObj.oldPath);
                    }
                    if (checkBox2.IsChecked == true)
                    {
                        valObj = new validationObject(double.Parse(length.Text), double.Parse(width.Text), double.Parse(height.Text), path.path[0], path.path[1]);
                        validate = false;
                        iteratePath();
                        System.IO.Directory.CreateDirectory(folderPath);
                        writeNewPath();
                        writeFiles(valObj.newPath);
                        //optimize path here
                    }
                }
            }
        }

        //iterate through path of nodes
        private Boolean iteratePath()
        {
            for (var i=1; i<path.path.Count; i++)
            {              
                valObj.rotateTrolley(path.path[i]);
                if (validate == false)
                {
                    Vector3D rotateCrash = clash();
                    if (rotateCrash.Length != 0)
                    {
                        if (!dynamicRelaxation(path.path[i - 1]))
                        {
                            return false;
                        }
                        else
                            testData.Items.Add("dynamic relaxation done for rotation: " + (i - 1).ToString());
                    }
                    valObj.newPath.Add(Tuple.Create(false, valObj.currentPosition)); //change node to the actuall new position from displacement
                }
                if (!dynamicRelaxation(path.path[i])) {
                    return false;
                }
                else
                    testData.Items.Add("dynamic relaxation done for node: " + i);

            }
            valObj.newPath.Add(Tuple.Create(false, valObj.currentPosition)); //change node to the actuall new position from displacement
            return true;
        }
            
        private Boolean dynamicRelaxation(Point3D nextNode)
        {
            int tolerance = 0;
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
                tolerance++;
                first=false; //check
                clashForces += clash();


                internalForce = valObj.K * displacement;
                dampingForce  = valObj.C * velocity;

                residualForce = -clashForces - internalForce - dampingForce;

                //new acceleration, velocity and the new change of displacement
                acceleration = residualForce / valObj.mass;
                velocity     += acceleration*deltaT;
                displacement += velocity * deltaT;

                valObj.updateObjectPosition(velocity * deltaT);
                if (tolerance == 500)
                {
                    testData.Items.Add("impossible crash node " + nextNode);
                    return false;
                }
            }
            valObj.oldPath.Add(Tuple.Create(crashingCheck, valObj.currentPosition));
            return true;
        }

        private void addCrashToPath(bool boo)
        {
            crashingCheck = boo;
            if (crashingCheck != crashing)
            {
                valObj.oldPath.Add(Tuple.Create(crashingCheck, valObj.currentPosition));
                crashing = crashingCheck;
            }
        }
        
        private Vector3D clash()
        {
            Vector3D forces = new Vector3D(0,0,0);
            int numberOfCrashes = 0;
            List<Tuple<List<int>, Point3D>> leftovers = new List<Tuple<List<int>, Point3D>>();
            foreach (Tuple<Vector3D, List<Point3D>> tri in model.boundary)
            {
                List<List<int>> intersectedLines = new List<List<int>>();
                List<Point3D> intersectionsPoints = new List<Point3D>();
                numberOfCrashes = 0;
                if (boundingBoxCheck(tri.Item2))
                {
                    continue;
                }
                else
                {
                    foreach(var line in valObj.linePoints)
                    {
                        if (Vector3D.DotProduct(tri.Item1, (valObj.trolley[line[0]] - valObj.trolley[line[1]])) != 0)
                        {
                            var t1 = Matrix<double>.Build.DenseOfArray(new[,] {
                                {1, 1, 1, 1 },
                                {tri.Item2[0].X, tri.Item2[1].X, tri.Item2[2].X, valObj.trolley[line[1]].X},
                                {tri.Item2[0].Y, tri.Item2[1].Y, tri.Item2[2].Y, valObj.trolley[line[1]].Y},
                                {tri.Item2[0].Z, tri.Item2[1].Z, tri.Item2[2].Z, valObj.trolley[line[1]].Z}});
                            var t2 = Matrix<double>.Build.DenseOfArray(new[,] {
                                {1, 1, 1, 0 },
                                {tri.Item2[0].X, tri.Item2[1].X, tri.Item2[2].X, valObj.trolley[line[0]].X-valObj.trolley[line[1]].X},
                                {tri.Item2[0].Y, tri.Item2[1].Y, tri.Item2[2].Y, valObj.trolley[line[0]].Y-valObj.trolley[line[1]].Y},
                                {tri.Item2[0].Z, tri.Item2[1].Z, tri.Item2[2].Z, valObj.trolley[line[0]].Z-valObj.trolley[line[1]].Z}});
                            var t = -t1.Determinant() / t2.Determinant();                        

                            if (0<t && t<1)
                            {
                                var intersect = new Point3D(valObj.trolley[line[1]].X + ((valObj.trolley[line[0]].X) - valObj.trolley[line[1]].X) * t, valObj.trolley[line[1]].Y + ((valObj.trolley[line[0]].Y) - valObj.trolley[line[1]].Y) * t, valObj.trolley[line[1]].Z + ((valObj.trolley[line[0]].Z) - valObj.trolley[line[1]].Z) * t);
                                if (insideTriangle(intersect, tri.Item2))
                                {
                                    if (validate == true)
                                    {
                                        addCrashToPath(true);
                                        return forces;
                                    }
                                    else if (validate == false)
                                    {
                                        intersectedLines.Add(line);
                                        intersectionsPoints.Add(intersect);
                                        numberOfCrashes++;
                                    }
                                }
                            }
                        }
                    }
                        
                }
                if (validate == false)
                {
                    foreach (var point in intersectedLines.Zip(intersectionsPoints, Tuple.Create))
                    {
                        var l = (valObj.trolley[point.Item1[0]]-point.Item2).Length;
                        var m = (valObj.trolley[point.Item1[1]] - point.Item2).Length;
                        if (l < m)
                        {
                            var distance = valObj.trolley[point.Item1[0]] - point.Item2;
                            if (distance.Length == Vector3D.DotProduct(distance, tri.Item1)){
                                forces += distance * kWall;
                            }
                        }
                    }
                    var nodeOutside = connectedLines(intersectedLines);
                    if (nodeOutside != -1)
                    {
                        var distance = Vector3D.DotProduct(tri.Item1, (valObj.trolley[nodeOutside] - tri.Item2[0]));
                        forces += new Vector3D(tri.Item1.X * distance, tri.Item1.Y * distance, tri.Item1.Z * distance) * kWall;
                        testData.Items.Add(distance);
                    }
                    testData.Items.Add("line intersections for 1 plane is: " + numberOfCrashes);
                    foreach(var f in intersectedLines.Zip(intersectionsPoints, Tuple.Create))
                    {
                        if (!f.Item1.Contains(nodeOutside))
                        {
                            leftovers.Add(f);
                        }
                    }
                }
                
            }
            if (validate == true)
            {
                addCrashToPath(false);
                return forces;
            }
            return forces;
        }
        private int connectedLines(List<List<int>> intersectedLines)
        {
            foreach (List<int> line1 in intersectedLines)
            {
                foreach (List<int> line2 in intersectedLines)
                {
                    if (!line1.Equals(line2) && line1.Intersect(line2).Any())
                    {
                        return line1.Intersect(line2).ToList()[0];
                    }
                }
            }
            return -1;
        }

        private Boolean boundingBoxCheck(List<Point3D> tri)
        {
            var sortX = valObj.trolley.OrderBy(point => point.X);
            var sortY = valObj.trolley.OrderBy(point => point.Y);
            var sortZ = valObj.trolley.OrderBy(point => point.Z);

            return tri[0].X > sortX.Last().X && tri[1].X > sortX.Last().X && tri[2].X > sortX.Last().X
                    || tri[0].X < sortX.First().X && tri[1].X < sortX.First().X && tri[2].X < sortX.First().X
                    || tri[0].Y > sortY.Last().Y && tri[1].Y > sortY.Last().Y && tri[2].Y > sortY.Last().Y
                    || tri[0].Y < sortY.First().Y && tri[1].Y < sortY.First().Y && tri[2].Y < sortY.First().Y
                    || tri[0].Z > sortZ.Last().Z && tri[1].Z > sortZ.Last().Z && tri[2].Z > sortZ.Last().Z
                    || tri[0].Z < sortZ.First().Z && tri[1].Z < sortZ.First().Z && tri[2].Z < sortZ.First().Z;

        }

        private bool insideTriangle(Point3D intersect, List<Point3D> tri)
        {
            Vector3D v0 = tri[2] - tri[0];
            Vector3D v1 = tri[1] - tri[0];
            Vector3D v2 = intersect - tri[0];

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

        


        private void writeFiles(List<Tuple<bool, Point3D>> writePath)
        {
            int filenum=0;
            valObj.start(writePath[0].Item2);
            valObj.rotateTrolley(writePath[1].Item2);

            for (var i = 0; i < writePath.Count-1; i++)
            {
                writeTrolleyFile(filenum, writePath[i].Item1);
                filenum++;
                if (writePath[i + 1].Item2 != writePath[i].Item2)
                {
                    valObj.rotateTrolley(writePath[i + 1].Item2);
                    valObj.updateObjectPosition(writePath[i + 1].Item2 - writePath[i].Item2);
                }
                else
                {
                    valObj.rotateTrolley(writePath[i + 2].Item2);
                }
            }
            writeTrolleyFile(filenum, writePath.Last().Item1);
            filenum++;
        }
        private void writeNewPath()
        {
            String[] startText = {"# vtk DataFile Version 4.0", "vtk output", "ASCII", "DATASET POLYDATA", "POINTS "+ valObj.newPath.Count + " double"};
            String endText = "LINES 1 " + (valObj.newPath.Count+1).ToString();
            using (StreamWriter file =
                        new StreamWriter(folderPath + @"\new path.vtk"))
            {
                foreach (var line in startText)
                {
                    file.WriteLine(line);

                }
                foreach(var node in valObj.newPath)
                {
                    file.WriteLine(node.Item2.X.ToString().Replace(",", "."));
                    file.WriteLine(node.Item2.Y.ToString().Replace(",", "."));
                    file.WriteLine(node.Item2.Z.ToString().Replace(",", "."));
                }
                file.WriteLine(endText);
                file.WriteLine(valObj.newPath.Count);
                
                for(var i=0; i<valObj.newPath.Count; i++)
                {
                    file.WriteLine(i);
                }
            }
            }
        private void writeTrolleyFile(int i, bool clash)
        {
            string nameFile;
            String[] startText = { "# vtk DataFile Version 4.0", "vtk output", "ASCII", "DATASET POLYDATA", "POINTS 8 float" };
            String[] endText = { "POLYGONS 6 30", "4 0 1 3 2 4 2 3 5 4 4 4 5 7 6 4 0 1 7 6 4 0 2 4 6 4 1 3 5 7" };
            String[] color = { "CELL_DATA 6", "SCALARS cell_scalars int 1", "LOOKUP_TABLE default", "0 1 2 3 4 5", "LOOKUP_TABLE default 6" };
            String green = "0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0 0.0 1.0";
            String red = "1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0 1.0 0.0 0.0 1.0";
            if (validate == true)
            {
                nameFile = "validate";
            }
            else
            {
                nameFile = "optimize";
            }
            
            using (StreamWriter file =
                        new StreamWriter(folderPath + @"\" + nameFile + i.ToString() + ".vtk"))
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

