using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace frontEnd
{
    class readPath
    {
        public List<Point3D> path = new List<Point3D>();

        public readPath(string pathName)
        {
            string[] lines = System.IO.File.ReadAllLines(pathName);
            int nodeNums = Int32.Parse(lines[4].Split(' ')[1]);

            // Display the file contents by using a foreach loop.
            for (var i = 5; i < 5 + Math.Ceiling((double)nodeNums / 3.0); i++)
            {

                String[] coords = lines[i].Trim().Split(' ');
                for (var j = 0; j < coords.Length - 2; j += 3)
                {
                    Point3D newNode = new Point3D();
                    newNode = Point3D.Parse(coords[j] + "," + coords[j + 1] + "," + coords[j + 2]);
                    path.Add(newNode);
                }
            }
        }

    }
}
