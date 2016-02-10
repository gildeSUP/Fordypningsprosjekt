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
        public double K { get; set; }
        public double C { get; set; }
        public double mass { get; set; }
        public List<Point3D> newPath { get; set; }

        //initialize object parameters
        public validationObject()
        {
            K = 0.1;
            C = 0.85; //2*Math.sqrt(m*sum(K))
            mass = 5;
            newPath = new List<Point3D>();
        }

        public Point3D getCurrentPossition()
        {
            return newPath.Last();
        }

        public void addNewPath(Point3D node)
        {
            newPath.Add(node);
        }

            
    }
}
