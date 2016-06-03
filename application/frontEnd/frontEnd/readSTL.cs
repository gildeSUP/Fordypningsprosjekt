using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace frontEnd
{
    class readSTL
    {
        public List<Tuple<Vector3D, List<Point3D>>> boundary { get; private set; }

        public readSTL(string file)
        {
            boundary = new List<Tuple<Vector3D, List<Point3D>>>();
            BinaryReader myFile = new BinaryReader(File.Open(file, FileMode.Open));

            myFile.ReadChars(80);
            var tris = myFile.ReadInt32();
            for (var i = 0; i < tris; i++)
            {
                var tri = new List<Point3D>();
                Vector3D normal = new Vector3D(myFile.ReadSingle(), myFile.ReadSingle(), myFile.ReadSingle());
                for (var j = 1; j < 4; j++)
                {
                    Point3D vec = new Point3D(myFile.ReadSingle(), myFile.ReadSingle(), myFile.ReadSingle());
                    tri.Add(vec);
                }
                myFile.ReadInt16();

                boundary.Add(Tuple.Create(normal, tri));
                Console.WriteLine(normal + ", " + tri[0] + ", " + tri[1] + ", " + tri[2]);
            }
            myFile.Close();
        }
    }
}
