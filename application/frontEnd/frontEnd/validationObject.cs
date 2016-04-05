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
        public double width { get; private set; }
        public double length { get; private set; }
        public double height { get; private set; }
        private double angleXY;
        public double angleXZ { get; private set; }
        public double K { get; private set; }
        public double C { get;  private set; }
        public double mass { get; private set; }
        public List<Point3D> newPath { get; private set; }
        public double nextAngleXY { get; private set; }

        //initialize object parameters
        public validationObject(double width, double length, double height, double mass, Point3D startPos)
        {
            angleXY = 0;
            angleXZ = 0;
            currentPosition = startPos;
            this.width = width;
            this.length = length;
            this.height = height;

            K = 0.1;
            this.mass = mass;
            C = 2 * Math.Sqrt(mass * K);

            newPath = new List<Point3D>();
            newPath.Add(startPos);

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

            nextAngleXY = Math.Atan2(nextNode.Y - currentPosition.Y, nextNode.X - currentPosition.X);
            double nextAngleXZ = Math.Atan2(nextNode.Z - currentPosition.Z, nextNode.X - currentPosition.X);

            rotatePoints(nextAngleXY, nextAngleXZ);
        }
        public void rotatePoints(double nextAngleXY, double nextAngleXZ)
        {
            if (nextAngleXY - angleXY != 0 && nextAngleXZ - angleXZ != 0)
            {
                Transform3DGroup group = new Transform3DGroup();
                group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), nextAngleXY - angleXY)));
                //group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), nextAngleXZ - angleXZ)));
                for (var i = 0; i < trolley.Count(); i++)
                {
                    trolley[i] = group.Transform(trolley[i]);

                }
                angleXY = nextAngleXY;
                angleXZ = nextAngleXZ;
            }
        }
        
            
    }
}
