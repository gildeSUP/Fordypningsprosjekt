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
        public double K { get; private set; }
        public double C { get;  private set; }
        public double mass { get; private set; }
        public List<Point3D> newPath { get; private set; }

        //initialize object parameters
        public validationObject(double width, double length, double mass, Point3D startPos)
        {
            currentPosition = startPos;
            this.width = width;
            this.length = length;

            K = 0.1;
            this.mass = mass;
            C = 2 * Math.Sqrt(mass * K);

            newPath = new List<Point3D>();
            newPath.Add(startPos);
        }


        public void updateObjectPosition(Vector3D distance)
        {
            currentPosition += distance;
        }
            
    }
}
