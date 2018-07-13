using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Hello
    {
        public struct point
        {
            public int water;
            public int hills;
            public int sidewalk;
            public point(int p1, int p2, int p3)
            {
                water = p1;
                hills = p2;
                sidewalk = p3;
            }
        }

        static void Main(string[] args)
        {
            int num_point_arrays = 2;

            point point1 = new point(1, 2, 0);
            point point2 = new point(3, 3, 1);

            point[] points1 = new point[] { point1, point2 };
            point[] points2 = new point[] { point2, point1 };

            point[][] sections = new point[num_point_arrays][]; //creates an array of point-arrays

            //places point-arrays that are just tests in sections
            sections[0] = points1;
            sections[1] = points2;
            
            Hashtable scannerResults = getScannerResults(points1);


            for (int i = 0; i < 2; i++)
            {


                foreach (DictionaryEntry entry in scannerResults)
                {
                    Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
                }
                Console.ReadKey();
            }
        }

        static Hashtable getScannerResults(point[] Points)
        {
            int weight_water = 0;
            int weight_hills = 0;
            int weight_sidewalk = 0;
            foreach (point p in Points)
            {
                weight_water += p.water;
                weight_hills += p.hills;
                weight_sidewalk += p.sidewalk;
            
            }
            
            Hashtable pointWeights = new Hashtable();
            pointWeights["water"] = weight_water;
            pointWeights["hills"] = weight_hills;
            pointWeights["sidewalk"] = weight_sidewalk;

            return pointWeights;

        }
    }

}
