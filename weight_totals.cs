using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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

        public struct ScannerData
        {
            public Hashtable weight_percentages;
            public List<List<point>> sections;
        }

        static void Main(string[] args)
        {
            string jsonString = "{ \"weight_percentages\":[{\"water\":0.75},{\"hills\":0.5}],\"sections\":[{\"id\":\"1\",\"points\":[{\"latitude\":11.7575,\"longitude\":13.55567,\"weights\":{\"water\":1,\"hills\":2}},{\"latitude\":12.2343,\"longitude\":2.553535,\"weights\":{\"water\":3,\"hills\":8}}]},{\"id\":\"9\",\"points\":[{\"latitude\":44.7898,\"longitude\":90.5898,\"weights\":{\"water\":6,\"hills\":7}},{\"latitude\":60.2676,\"longitude\":32.553535,\"weights\":{\"water\":5,\"hills\":6}}]}]}";

            List<double> section_totals = scanSections(jsonString);

            printHighestWeightedTotal(section_totals);

            Console.ReadKey();
        }


        //scans the json string and returns the weighted totals each section under the "sections" key of the json string
        public static List<double> scanSections(string jsonString)
        {
            ScannerData data = parseJsonData(jsonString);

            List<List<point>> sections = data.sections;

            List<Hashtable> section_results = getSectionResults(sections);
                        
            List<double> section_totals = getSectionTotals(section_results, data.weight_percentages);

            return section_totals;
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

        //helper function that gets the highest waited total
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

        //helper function that prints the highest weighted total and its section number
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

        //parses a json string and places desired information into a ScannerData struct
        static ScannerData parseJsonData(string jsonString)
        {
            ScannerData returnData;

            returnData.weight_percentages = Get_Weight_Percentages(jsonString);
            returnData.sections = Get_Values_For_Each_Section(jsonString);

            return returnData;

            //sample json string

            //{"weight_percentages":[{"water":0.75},{"hills":0.5}],"sections"
            //:[{"id":"1","points":[{"latitude":11.7575,"longitude":13.55567,
            //"weights":{"water":1,"hills":2}},{"latitude":12.2343,"longitude":
            // 2.553535,"weights":{"water":3,"hills":8}}]},
            // {"id":"9","points":[{"latitude":44.7898,"longitude":90.5898,"weights":
            // {"water":6,"hills":7}},{"latitude":60.2676,"longitude"
            // :32.553535,"weights":{"water":5,"hills":6}}]}]}

        }

        //gets the weight percentage for each object from the json string
        static Hashtable Get_Weight_Percentages(string jsonString)
        {
            //takes the json string and makes it readable when using the newtonsoft.json namespace
            JObject results = JObject.Parse(jsonString);

            //creates a temporary dictionary to eventually store the weight percentages and move it to a more permanent dictionary
            Dictionary<string, double> temp = new Dictionary<string, double>();

            //the dictionary that will store all the necessary data and that will be made into the final hashtable
            Dictionary<string, double> weight_percents = new Dictionary<string, double>();

            //runs a loop through each element within the "weight_percentages" portion of the json string
            foreach (var element in results["weight_percentages"])
            {
                //takes the section of the json object and converts it into a dictionary
                //so this portion would take something like {"water": 0.75} and put it in a dictionary 
                temp = JsonConvert.DeserializeObject<Dictionary<string, double>>(Convert.ToString(element));

                //adds the elements in the temporary dictionary to the official one
                foreach (KeyValuePair<string, double> item in temp)
                {
                    weight_percents.Add(item.Key, item.Value);
                }

            }

            //Sidewalk is absent in the json code, so I put it at zero. Because the point struct
            //contains a sidewalk portion, I needed some percentage value for sidewalk otherwise I'd
            //get a bug
            double sidewalk_percent = 0;
            weight_percents.Add("sidewalk", sidewalk_percent);

            //the dictionary is now a hashtable
            Hashtable returnHashtable = new Hashtable(weight_percents);

            return returnHashtable;
        }

        //Gets the value for each object within a point (i.e. "water": 1) and places it in a 2d array ->
        //for each section, there is a list of points.
        static List<List<point>> Get_Values_For_Each_Section(string jsonString)
        {
            //so that I can use the newtonsoft.json namespace to help
            JObject results = JObject.Parse(jsonString);

            //makes temporary 2d array and temporary 1d array
            List<List<point>> sections = new List<List<point>>();
            List<point> list = new List<point>();

            //creates a string that only contains the information under "sections" (from the json string)
            string sections_string = results["sections"].ToString();

            //stores the information under "sections" as a json array that I can use for my benefit
            JArray sections_array = JArray.Parse(sections_string);

            //will eventually have the specific information under "points" (from the json string)
            string points_string;

            //finds the number of arrays within "sections"
            //this will be helpful in a for loop as I locate each specific value
            int num_sections = sections_array.Count;

            //where the values for each object will be stored
            int water, hills;
            int sidewalk = 0;

            //there may be multiple json arrays further within "sections" under "points
            //this is used as a temporary json array
            JArray points_array;

            //same use as num_sections, but specific to the amount of elements within "points"
            //there are multiple "points" within sections so I don't know the value yet
            int num_points;

            //temporary point that will transfer points into the 2d array
            point place;


            //for each section
            for (int i = 0; i < num_sections; i++)
            {
                //resets the array to accept new data
                list = new List<point>();

                //finds the information under the "points" key in the json string and stores it as a string
                points_string = results["sections"][i]["points"].ToString();

                //parses the array elements under "points"
                points_array = JArray.Parse(points_string);

                //finds the amount of arrays within the new JArray and uses that in the next for loop
                num_points = points_array.Count;

                for (int j = 0; j < num_points; j++)
                {
                    //by querying the specific values of "water" and "hills" can be found
                    //the JArray objects above were important in finding the values of i and j
                    water = (int)results["sections"][i]["points"][j]["weights"]["water"];
                    hills = (int)results["sections"][i]["points"][j]["weights"]["hills"];

                    //cretes a point with the obtained data and adds it to a list (array)
                    place = new point(water, hills, sidewalk);
                    list.Add(place);
                }
                //adds the new list to the larger 2d array
                sections.Add(list);
            }

            return sections;
        }

        //places the scanner results of each section of points into a list of hashtables
        static List<Hashtable> getSectionResults(List<List<point>> sections)
        {
            List<Hashtable> section_results = new List<Hashtable>();

            //places the Scanner results from each pointer-array into section_results
            foreach (List<point> subList in sections)
            {
                section_results.Add(getScannerResults(subList));
            }

            return section_results;
        }

        //takes each Hashtable in a list and finds the weighted total of each section. This is done by multiplying each result to 
        //it's respective weight percentage and then adding it into a total. This total is stored in a list of totals for each section.
        static List<double> getSectionTotals(List<Hashtable> section_results, Hashtable weight_percentages)
        {
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
                }
                section_totals.Add(total);
            }
            return section_totals;
        }

    }

}
