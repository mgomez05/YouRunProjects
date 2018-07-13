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
            point point1 = new point(1, 2, 0);
            point point2 = new point(3, 3, 1);
            point[] points = new point[] { point1, point2 };
            Hashtable scannerResults = getScannerResults(points);
               
            foreach (DictionaryEntry entry in scannerResults)
            {
                Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
            }
            Console.ReadKey();
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
