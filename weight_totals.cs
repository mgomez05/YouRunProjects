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
            /*
            int num_point_arrays = 3;
            
            //test values
            point point1 = new point(1, 2, 0);
            point point2 = new point(3, 3, 1);
            point point3 = new point(10, 10, 10);

            point[] points1 = new point[] { point1, point2 };
            point[] points2 = new point[] { point2, point1 };
            point[] points3 = new point[] { point1, point3 };
            
            //STEP 1

            //2d array of point-arrays
            point[][] sections = new point[num_point_arrays][]; 

            //places point-arrays that are just tests in sections
            sections[0] = points1;
            sections[1] = points2;
            sections[2] = points3;

            //STEP 2

            //an array of Hashtables that contains the results from getScannerResults
            Hashtable[] section_results = new Hashtable[num_point_arrays];
            int j = 0;

            //places the Scanner results from each pointer-array into section_results
            foreach (point[] array in sections)
            {
                section_results[j] = getScannerResults(array);
                j++;
            }
            */

            //STEP 3

            //A new hashtable with weight percentages for objects
            Hashtable weight_percentages = new Hashtable();

            weight_percentages.Add("water", 0.75);
            weight_percentages.Add("hills", 0.25);
            weight_percentages.Add("sidewalk", 0.50);

            //just checks that I can cast the type
            double check_if_double = (double)weight_percentages["sidewalk"];

            Console.WriteLine(check_if_double);

            /*

            //runs a loop to output the information to see if the program worked
            for (int i = 0; i < num_point_arrays; i++)
            {

                foreach (DictionaryEntry entry in section_results[i])
                {
                    Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
                }
                
            }
            */
            Console.ReadKey();
        }
        
        //takes an array of pointers and stores the information in a hashtable
        //also adds the total values of all of the pointers
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
