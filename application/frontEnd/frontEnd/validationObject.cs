using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace frontEnd
{
    class validationObject
    {
        public Point3D currentPosition { get; private set; }
        public List<Point3D> trolley { get; private set; }
        public List<Point3D> startTrolley { get; private set; }
        public List<List<int>> linePoints { get; private set; }//dict with the index to create all lines of the box
        public double width { get; private set; }
        public double length { get; private set; }
        public double height { get; private set; }
        public double angleXY { get; private set; }
        public double angleXZ { get; private set; }
        public double angleYZ { get; private set; }
        public double K { get; private set; }
        public double C { get;  private set; }
        public double mass { get; private set; }
        public List<Tuple<bool, Point3D>> newPath { get; private set; }
        public List<Tuple<bool, Point3D>> oldPath { get; private set; }
        public double nextAngleXY { get; private set; }
        public double nextAngleXZ { get; private set; }
        public double nextAngleYZ { get; private set; }
        
        //initialize object parameters
        public validationObject(double length, double width, double height, Point3D startPos, Point3D rotateToPoint)
        {

            this.width = width;
            this.length = length;
            this.height = height;

            K = 0.1; //stiffness
            this.mass = 5;
            C = 2 * Math.Sqrt(mass * K); //damping coefficient

            newPath = new List<Tuple<bool, Point3D>>();
            oldPath = new List<Tuple<bool, Point3D>>();
            oldPath.Add(Tuple.Create(false, startPos));

            start(startPos);
            
            rotateTrolley(rotateToPoint);
            trolleyLines();
        }
        public void start(Point3D startPos)
        {
            angleXY = 0;
            angleXZ = 0;
            angleYZ = 0;
            currentPosition = startPos;
            trolley = new List<Point3D>();
            trolley.Add(new Point3D(startPos.X + (length / 2), startPos.Y + (width / 2), startPos.Z + (height / 2)));
            trolley.Add(new Point3D(startPos.X + (length / 2), startPos.Y + (width / 2), startPos.Z - (height / 2)));
            trolley.Add(new Point3D(startPos.X + (length / 2), startPos.Y - (width / 2), startPos.Z + (height / 2)));
            trolley.Add(new Point3D(startPos.X + (length / 2), startPos.Y - (width / 2), startPos.Z - (height / 2)));
            trolley.Add(new Point3D(startPos.X - (length / 2), startPos.Y - (width / 2), startPos.Z + (height / 2)));
            trolley.Add(new Point3D(startPos.X - (length / 2), startPos.Y - (width / 2), startPos.Z - (height / 2)));
            trolley.Add(new Point3D(startPos.X - (length / 2), startPos.Y + (width / 2), startPos.Z + (height / 2)));
            trolley.Add(new Point3D(startPos.X - (length / 2), startPos.Y + (width / 2), startPos.Z - (height / 2)));
        }

        public void updateObjectPosition(Vector3D distance)
        {
            currentPosition += distance;
            for (var i = 0; i < trolley.Count(); i++)
            {
                trolley[i] += distance;
            }
        }

        //TODO
        public void rotateTrolley(Point3D nextNode) 
        {

            nextAngleXY = Math.Atan2(nextNode.Y - currentPosition.Y, nextNode.X - currentPosition.X) * (180/Math.PI);
            nextAngleXZ = Math.Atan2(nextNode.Z - currentPosition.Z, nextNode.X - currentPosition.X) * (180 / Math.PI);
            nextAngleYZ = Math.Atan2(nextNode.Z - currentPosition.Z, nextNode.Y - currentPosition.Y) * (180 / Math.PI);           

            rotatePoints(nextAngleXY, nextAngleXZ, nextAngleYZ);
        }
        public void rotatePoints(double nextAngleXY, double nextAngleXZ, double nextAngleYZ)
        {
            if (nextAngleXY - angleXY != 0 || nextAngleXZ - angleXZ != 0 || nextAngleYZ - angleYZ != 0)
            {
                
                var group = new Transform3DGroup();
                var myRotateTransformZ = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), nextAngleXY-angleXY), currentPosition);
                group.Children.Add((myRotateTransformZ));
                /*var myRotateTransformY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), nextAngleXZ - angleXZ), currentPosition);
                group.Children.Add((myRotateTransformY));
                var myRotateTransformX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), nextAngleYZ - angleYZ), currentPosition);
                group.Children.Add((myRotateTransformX));*/
                for (var i = 0; i < trolley.Count(); i++)
                {
                    trolley[i] = group.Transform(trolley[i]);

                }
                angleXY = nextAngleXY;
                angleXZ = nextAngleXZ;
                angleYZ = nextAngleYZ;
            }
        }
        //index of every line created of the trolleyPoints
        public void trolleyLines() 
        {
            linePoints = new List<List<int>>();
            linePoints.Add(new List<int> { 0, 1 });
            linePoints.Add(new List<int> { 0, 2 });
            linePoints.Add(new List<int> { 0, 6 });
            linePoints.Add(new List<int> { 1, 3 });
            linePoints.Add(new List<int> { 1, 7 });
            linePoints.Add(new List<int> { 2, 3 });
            linePoints.Add(new List<int> { 4, 2 });
            linePoints.Add(new List<int> { 4, 5 });
            linePoints.Add(new List<int> { 4, 6 });
            linePoints.Add(new List<int> { 5, 3 });
            linePoints.Add(new List<int> { 5, 7 });
            linePoints.Add(new List<int> { 6, 7 });
        }


    }
}
