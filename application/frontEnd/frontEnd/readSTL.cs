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
        public List<List<Vector3D>> boundary { get; private set; }

        public readSTL(string file)
        {
            boundary = new List<List<Vector3D>>();
            BinaryReader myFile = new BinaryReader(File.Open(file, FileMode.Open));


            var header = new string(myFile.ReadChars(80));
            var triangles = myFile.ReadInt32();
            for (var i = 0; i < triangles; i++)
            {
                List<Vector3D> triangle = new List<Vector3D>();
                for (var j = 0; j < 4; j++)
                {
                    Vector3D vec = new Vector3D(myFile.ReadSingle(), myFile.ReadSingle(), myFile.ReadSingle());
                    triangle.Add(vec);
                }
                myFile.ReadInt16();
                boundary.Add(triangle);
            }
            myFile.Close();
            Console.WriteLine(boundary);
        }
    }
}
