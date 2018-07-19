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
            //STEP 5                       
                     
            //test values
            point point1 = new point(1, 2, 0);
            point point2 = new point(3, 3, 1);
            point point3 = new point(10, 10, 10);
            point point4 = new point(50, 0, 7);
            point point5 = new point(5, 11, 3);
            
            List<point> points1 = new List<point>
            {
                point1,
                point2
            };
            List<point> points2 = new List<point>
            {
                point5,
                point1
            };
            List<point> points3 = new List<point>
            {
                point1,
                point3
            };
            List<point> points4 = new List<point>
            {
                point4,
                point5
            };
                              
            //A list of lists that acts as a dynamic jagged array
            List<List<point>> sections = new List<List<point>>();
            sections.Add(points1);
            sections.Add(points2);
            sections.Add(points3);
            sections.Add(points4);
                      
            
            //a list of Hashtables that contains the results from getScannerResults
            List<Hashtable> section_results = new List<Hashtable>();
        
            

            //places the Scanner results from each pointer-array into section_results
            foreach (List<point> subList in sections)
            {
                section_results.Add(getScannerResults(subList));                                
            }

            //A new hashtable with weight percentages for objects
            Hashtable weight_percentages = new Hashtable();

            weight_percentages.Add("water", 0.75);
            weight_percentages.Add("hills", 0.25);
            weight_percentages.Add("sidewalk", 0.50);

            //a list containing the total values for each section when the weight percentages are factored in
            List<double> section_totals = new List<double>();

            
            //runs a loop to add the totals from section_results into section_totals
            foreach (Hashtable hashtables in section_results)
            {
                double total = 0;

                foreach (DictionaryEntry entry in hashtables)
                {
                    //makes the key in each DictionaryEntry a string and the Value a double
                    string key = (string)entry.Key;
                    double value_for_landmark = Convert.ToDouble(entry.Value);
                                        
                    //adds the result of multiplying the weight percentages to the values for each landmark
                     total += ((double)weight_percentages[key] * value_for_landmark);

                    //checks if the math adds up
                    //Console.WriteLine("This is the int value {0}", entry.Value);
                   // Console.WriteLine("This is the weight_percentage {0}",(double)weight_percentages[key]);
                   // Console.WriteLine("This is the value times the weight_percentage {0}", (double)weight_percentages[key] * value_for_landmark);
                   //Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
                }
                section_totals.Add(total);
            }
            printHighestWeightedTotal(section_totals);
            Console.ReadKey();
                 }
        
        //takes an array of pointers and stores the information in a hashtable
        //also adds the total values of all of the pointers
        static Hashtable getScannerResults(List<point> Points)
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
        static double getHighestWeightedTotal(List<double> section_totals)
        {
            double highest_weighted_total = 0;
            double section_total_before = 0;
            foreach (double weighted_total in section_totals)
            {
                if (weighted_total > section_total_before)
                {
                    highest_weighted_total = weighted_total;
                }
                section_total_before = weighted_total;
            }
            return highest_weighted_total;
        }

        static void printHighestWeightedTotal(List<double> section_totals)
        {
            double highest_weighted_total = getHighestWeightedTotal(section_totals);
            int highest_section = 1;
            double section_total_before = 0;
            int section_number = 1;

            foreach (double weighted_total in section_totals)
            {

                if (weighted_total > section_total_before)
                {
                    highest_section = section_number;
                }
                section_number++;
            }
            Console.WriteLine("Section #{0} has the highest weighted total of {1}", highest_section, highest_weighted_total);
        }
       
    }

}
