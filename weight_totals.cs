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
            

            //STEP 3

            //A new hashtable with weight percentages for objects
            Hashtable weight_percentages = new Hashtable();

            weight_percentages.Add("water", 0.75);
            weight_percentages.Add("hills", 0.25);
            weight_percentages.Add("sidewalk", 0.50);

            
            //STEP 4

            //an array containing the total values for each section when the weight percentages are factored in
            double[] section_totals = new double[num_point_arrays];

            //runs a loop to output the information to see if the program worked
            for (int i = 0; i < num_point_arrays; i++)
            {
                
                
                foreach (DictionaryEntry entry in section_results[i])
                {
                    //makes the key in each DictionaryEntry a string and the Value a double
                    string key = (string)entry.Key;     
                    double value_for_landmark = Convert.ToDouble(entry.Value);

                    //adds the result of multiplying the weight percentages to the values for each landmark
                    section_totals[i] += ((double)weight_percentages[key] * value_for_landmark);
                    
                    //checks if the math adds up
                    Console.WriteLine("This is the int value {0}", entry.Value);
                    Console.WriteLine("This is the weight_percentage {0}",(double)weight_percentages[key]);
                    Console.WriteLine("This is the value times the weight_percentage {0}", (double)weight_percentages[key] * value_for_landmark);
                   // Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
                }
                Console.WriteLine("Section {0} totals equals {1}", i + 1, section_totals[i]);
            }
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
