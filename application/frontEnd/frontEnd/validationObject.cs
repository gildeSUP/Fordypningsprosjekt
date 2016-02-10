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
        public List<Point3D> newPath = new List<Point3D>();

        public int lol { get; set; }

        public validationObject()
        {
            K = 0.1;
            C = 0.85; //2*Math.sqrt(m*sum(K))
            mass = 5;
        }
        public Point3D getCurrentPossition()
        {
            return newPath.Last();
        }
    }
}
