using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Data;

namespace Data
{
    class create_route
    {
        //used in scanSections() function because that does not use longitude or latitude values
        public struct point
        {            
            public Hashtable weights;
        }
  
        public struct ScannerData
        {
            public Hashtable weight_percentages;
            public List<List<point>> sections;
        }

        //stores the longitude, latitude, and weight of a specific point
        public struct Point
        {
            public double longitude;
            public double latitude;
            public Hashtable weights;
        }

        //a triangular slice of a square that contains the three points that make up its edges, and the points within the area of the triangle
        public struct SectionSlice
        {
            public List<Point> triangleBounds;// The bounds of this section of the 
                                              // runner area. As the section is a 
                                              // triangle, this list should contain only 
                                              // 3 points

            public List<Point> memberPoints;  // The points that are geographically 
                                              // located in this section

        }

        static void Main(string[] args)
        {
            //json with the designated location and the preferences that will be used to find the best section
            string jsonString = "{ \"preferences\": [{\"name\": \"hills\", \"weight\": .75},{ \"name\": \"water\", \"weight\": .55}, { \"name\": \"grass\", \"weight\": -.44}], \"current location\": { \"lat\": 0.99, \"lng\": 3.99}, \"radius\": 20}";

            List<Point> best_section = findBestRunningArea(jsonString);

            foreach (Point point in best_section)
            {
                Console.WriteLine("This is a point in the best section Lat: {0}, Lng: {1}", point.latitude, point.longitude);
            }

            Console.ReadKey();

        }

        static List<Point> findBestRunningArea(string jsonString)
        {
            //a bunch of get functions to be used later for divideSections
            Hashtable preferences = Get_Preferences(jsonString);

            double radius = Get_Radius(jsonString);

            Point currentLocation = Get_currentLocation(jsonString);

            List<double> boundries = Get_squareBoundary(currentLocation, radius);

            //the points obtained are of any latitude and longitude as of now (0 -> 180)
            List<Point> areaPoints = HTTP_Get_Points(180, 0, 180, 0);   

            List<List<Point>> sections = divideSections(areaPoints, 4, boundries[0], boundries[1], boundries[2], boundries[3], currentLocation);

            //creates a jsonString to be analized by scanSections
            string new_jsonString = create_JSON_string(preferences, sections);

            //the weighted totals of each section
            List<double> scan_result = scanSections(new_jsonString);

            //a list of points that are within the best section
            List<Point> best_section = findBestSection(scan_result, sections);

            printHighestWeightedTotal(scan_result);

            return best_section;

        }
               
        //creates a json string in a specific format
        //this json string will contain information about the weight percentages, and the specifics of each section
        static string create_JSON_string(Hashtable preferences, List<List<Point>> sections)
        {
            //JTokenWriter is basically a tool that allows the user to manually input each part of the json
            //Passed by reference and I like to use it since it allows me to run for loops unlike other serializing newtonsoft stuff
            JTokenWriter writer = new JTokenWriter();

            //startObject = {
            writer.WriteStartObject();

            //helper functions
            Write_Weight_Percentages(preferences, writer);
            Write_Sections(sections, writer);

            //EndObject = }
            writer.WriteEndObject();

            //placed into a JObject and then cast into a string
            JObject o = (JObject)writer.Token;
            string json = o.ToString();

            return json;

        }

        //creates the weight_percentages portion of a json string, passed by reference
        static void Write_Weight_Percentages(Hashtable preferences, JTokenWriter writer)
        {   
            //Example of PropertyName and Value: PropertyName -> "weight_percentages" :
            writer.WritePropertyName("weight_percentages");
            //StartArray = [
            writer.WriteStartArray();

            //writes the hashtable in json format
            foreach (DictionaryEntry entry in preferences)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(entry.Key.ToString());
                writer.WriteValue(Convert.ToDouble(entry.Value));
                writer.WriteEndObject();
            }
            //EndArray = ]
            writer.WriteEndArray();               
        }

        //writes the Section portion of the json string, passed by reference
        static void Write_Sections(List<List<Point>> sections, JTokenWriter writer)
        {
            writer.WritePropertyName("sections");
            writer.WriteStartArray();

            //I made the id number equal to the index number
            int id_number = 1;

            //example of what each element of the foreach loop looks like:
            //{"id":"1","points":[{"latitude":11.7575,"longitude":13.55567,
            //"weights":{"water":1,"hills":2}},{"latitude":12.2343,"longitude":
            // 2.553535,"weights":{"water":3,"hills":8}}
            foreach (List<Point> list in sections)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(id_number);
                writer.WritePropertyName("points");
                writer.WriteStartArray();
                foreach (Point point in list)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("latitude");
                    writer.WriteValue(point.latitude);
                    writer.WritePropertyName("longitude");
                    writer.WriteValue(point.longitude);
                    writer.WritePropertyName("weights");
                    writer.WriteStartObject();
                    foreach (DictionaryEntry hash in point.weights)
                    {                        
                        writer.WritePropertyName(hash.Key.ToString());
                        writer.WriteValue(Convert.ToDouble(hash.Value));
                        Console.WriteLine("{0}, {1}", hash.Key, hash.Value);
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                id_number++;
            }

            writer.WriteEndArray();
        }

    



        //gets the weight percentage for each object from the json string
        static Hashtable Get_Preferences(string jsonString)
        {
            //takes the json string and makes it readable when using the newtonsoft.json namespace
            JObject results = JObject.Parse(jsonString);

            //Isolates everything under "preferences" and makes it a string
            string preferences_string = results["preferences"].ToString();

            //parse the information under "preferences" as a JArray in order to find how many preferences there are
            JArray preferences = JArray.Parse(preferences_string);

            int num_preferences = preferences.Count;

            Hashtable returnHashtable = new Hashtable();

            //places the relevant information from each preference into a hashtable
            for (int i = 0; i < num_preferences; i++)
            {
                returnHashtable.Add(preferences[i]["name"], preferences[i]["weight"]);
               
            }

            return returnHashtable;
        }

        //gets the specified "radius" from a json string
        static double Get_Radius(string jsonString)
        {
            JObject results = JObject.Parse(jsonString);

            double radius = Convert.ToDouble(results["radius"]);

            return radius;
        }

        //gets the latitude and longitude values of the specified "current location" in a json string
        static Point Get_currentLocation(string jsonString)
        {
            JObject results = JObject.Parse(jsonString);

            Point returnLocation;

            returnLocation.latitude = Convert.ToDouble(results["current location"]["lat"]);
            returnLocation.longitude = Convert.ToDouble(results["current location"]["lng"]);
                        
            returnLocation.weights = new Hashtable();

            return returnLocation;

        }

        //Using a specific location as the center and a radius, this function creates a square.
        //This square is made up of four corner points: a maximum latitude, a minimum latitude, 
        //a maximum longitude, and a minimum longitude, which are returned as a list of doubles in that 
        //same order.
        static List<double> Get_squareBoundary(Point currentLocation, double radius)
        {   
            //change in latitude and longitude from the currentLocation will determine the corners of the square
            double change_in_latitude = Convert_miles_to_latitude(radius);
            double change_in_longitude = Convert_miles_to_longitude(currentLocation, radius);

            //square made up of max and min values of longitude and latitude.
            //Think of longitude and latitude as x and y coordinates
            double maxLat = currentLocation.latitude + change_in_latitude;
            double minLat = currentLocation.latitude - change_in_latitude;
            double maxLng = currentLocation.longitude + change_in_longitude;
            double minLng = currentLocation.longitude - change_in_longitude;

            List<double> returnBoundaries = new List<double>();
            returnBoundaries.Add(maxLat);
            returnBoundaries.Add(minLat);
            returnBoundaries.Add(maxLng);
            returnBoundaries.Add(minLng);

            return returnBoundaries;
        }

        //converts the unit of miles into latitude
        static double Convert_miles_to_latitude(double radius)
        {
            double earth_radius = 3960.0;
            double radians_to_degrees = 180.0 / Math.PI;

            double change_in_latitude = (radius / earth_radius) * radians_to_degrees;

            return change_in_latitude;
        }
           
        //converts the unit of miles into longitude
        //longitude is a little bit more weird to convert than latitude because the earth is not a perfect sphere
        //this accounts for the earth's more ellipsoid shape
        static double Convert_miles_to_longitude(Point currentLocation, double radius)
        {
            double earth_radius = 3960.0;
            double degrees_to_radians = Math.PI / 180.0;
            double radians_to_degrees = 180.0 / Math.PI;         

           
            double r = earth_radius * Math.Cos(currentLocation.latitude * degrees_to_radians);
            double change_in_longitude = (radius / r) * radians_to_degrees;

            return change_in_longitude;
        }

        //runs a HTTP get request in order to get the information from the server
        //it then places the information into a list of points called arePoints
        static List<Point> HTTP_Get_Points(double maxLat, double minLat, double maxLng, double minLng)
        {
            // Create a request using a URL that can receive a post
            WebRequest request = WebRequest.Create("https://yourun-server.herokuapp.com/locations/range?minLat=" + minLat.ToString() + "&maxLat=" + maxLat.ToString() + "&minLng=" + minLng.ToString() + "&maxLng=" + maxLng.ToString());
            // Set the Method property of the request to POST.
            request.Method = "Get";
            // Create POST data and convert it to a byte array.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string runnerAreaJson = reader.ReadToEnd();
            // Clean up the streams and the response.
            reader.Close();
            response.Close();

            //parses the json string to get a list of points
            List<Point> areaPoints = Parse_runnerAreaJson(runnerAreaJson);

           
            return areaPoints;
        }

        //Parses the json string obtained from the server so that each point in the json string
        //can be stored into a list
        static List<Point> Parse_runnerAreaJson(string runnerAreaJson)
        {
            //makes the json string readable through the newtonsoft namespace
            JObject results = JObject.Parse(runnerAreaJson);

            //finds the amount of elements there are within the key "results" (stored as num_points)
            string results_string = results["results"].ToString();
            JArray json_points_array = (JArray)results["results"];
            int num_points = json_points_array.Count;

            //declared now to be used later
            JArray weights_array;
            int num_weights;
            Point temp;
            List<Point> areaPoints = new List<Point>();

            //runs through each element within "results" and stores the information 
            //under "latitude", "longitude", and "weights" into a point. This point is then 
            //added to a list of points
            for (int i = 0; i < num_points; i++)
            {                
                temp.latitude = Convert.ToDouble(results["results"][i]["latitude"]);
                temp.longitude = Convert.ToDouble(results["results"][i]["longitude"]);

                temp.weights = new Hashtable();
                //finds the amount of elemetns there are within "weights"
                weights_array = (JArray)results["results"][i]["weights"];
                num_weights = weights_array.Count;
                
                //runs through each element within "weights" and stores that information
                //in a hashtable
                for (int j = 0; j < num_weights; j++)
                {     
                    temp.weights.Add(results["results"][i]["weights"][j]["name"], results["results"][i]["weights"][j]["value"]);
                    
                }
                areaPoints.Add(temp);
            }
            
            return areaPoints;
        }

        //This function takes a list of points, a number of sections, a set of latitudes and longitudes that determine two opposite edges of a square,
        //and the center of this square. It slices the square into triangular pieces of equal area based of the amount of sections requested, and then
        //takes the list of points and tries to see which section each point belongs in. It returns the a list of a list of points that contains 
        //each section and what points are within each section.
        static List<List<Point>> divideSections(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {
            //these variables are for counting and outputting purposes
            int j = 1;
            int i;
            int z = 1;

            List<SectionSlice> sectionSlices = SlicetheSquarePie(areaPoints, numSections, maxLat, minLat, maxLng, minLng, center);

            //runs a loop through for each point in the areaPoints list and checks if it is within the bounds of a sectionSlice
            for (int k = 0; k < areaPoints.Count; k++)
            {
                Point currentPoint = areaPoints[k];
                i = 1;              

                //For each slice, it checks if the current point is within the slice and if so it is stored as a memberpoint in that slice.
                foreach (SectionSlice slice in sectionSlices)
                {                    
                    if (IsPointInBounds(currentPoint, slice) == true)
                    {                        
                        slice.memberPoints.Add(currentPoint);
                        //Console.WriteLine("Point {0} is within bounds of section {1}", z, i);
                        //Console.WriteLine("");
                        break;
                    }
                    i++;
                }

                //If the previous loop never breaks, then the point was not within any section and is discarded.
                //The counting variable k needs to be subtracted from to account for the change in the list.
                if (i > numSections)
                {
                    //Console.WriteLine("point is not within selected area");
                    areaPoints.Remove(currentPoint);
                    k--;
                }
                z++;
            }

            //creates a List of Lists called sections and then stores the section and its memberpoints within sections
            List<List<Point>> sections = new List<List<Point>>();
            foreach (SectionSlice slice in sectionSlices)
            {
                sections.Add(slice.memberPoints);
            }
            /*
            foreach (SectionSlice slice in sectionSlices)
            {
                Console.WriteLine("Section #{0}", j);
                foreach (Point point in slice.triangleBounds)
                {
                    Console.WriteLine("longitude: {0}, latitude: {1}", point.longitude, point.latitude);
                }
                j++;
            }
            */          
            
            return sections;       
        }

        //This is a long function, so bear with me. 
        //The function slices a square made up of max and min latitudes and longitudes into any number of triangular sections.
        //Each triangle has three points: two on the perimeter of the square, and one at the center.
        //This function finds the points that is required to make however many sections and returns a list of SectionSlices that contains the bounds
        //for each triangle based on these points.
        //It does this by knowing the side lengths of the square and the perimeter.
        //By dividing the perimeter by the number of sections, the length between the two points on the perimeter of the square that make up a section
        //is known. The function starts at the min latitude and the min longitude, and works its way around the perimeter to find a point along
        //the perimeter that is a distance equal to the perimeter divided by the number of sections away. These two points along with the center is the 
        //first section. Then from the second point (so not minLat and minLng) it finds the next point that is the same distance away. It keeps doing this
        //until there are the correct number of sections. 
        static List<SectionSlice> SlicetheSquarePie(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {

            List<SectionSlice> sectionSlices = new List<SectionSlice>();
            
            //two temporary points will be used to eventually store information into sectionSlices.triangleBounds
            Point point1;
            Point point2;

            SectionSlice slice;
            //side length of the square
            double side_length = maxLat - minLat;
            //perimeter of the square
            double perimeter = side_length * 4;
            //distance between two points on the perimeter of the square that makes up a section
            double distance_between_points = perimeter / numSections;

          
            //to keep track of where we are along the perimeter
            double current_distance = 0;
            double prev_distance;
            double current_latitude = 0;
            double current_longitude = 0;

            //if numSections is less than three than those are special cases since using the center as a point
            //of a triangle bound would not work
            if (numSections >= 3)
            {
                //loop that keeps track of how many sections there are
                for (int i = 0; i < numSections; i++)
                {
                    slice.triangleBounds = new List<Point>();

                    //this is the first side of the square that the function travels through.
                    //this side will always have a latitude value of minLat.
                    //it finds the first point on the perimeter and then adds the distance_between_points
                    //to find the second.
                    if (current_distance < (perimeter / 4))
                    {
                        current_longitude = minLng + current_distance;
                        current_latitude = minLat;
                        point1.longitude = current_longitude;
                        point1.latitude = current_latitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;
                        point2 = Find_Second_Point_1stSide(current_distance, distance_between_points, minLng, minLat, maxLng, perimeter);
                    
                    //when finding the second point, the function needs to know if the current_distance is still on the same
                    //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
                    //it moved to another side.
                    /*if (current_distance <= (perimeter / 4))
                    {
                        current_longitude = minLng + current_distance;
                        point2.longitude = current_longitude;
                        point2.latitude = minLat;

                    }
                    //If it moved to the next side, the longitude stays at maxLng. However, the latitude has to change
                    //based off how much distance it traveled on the previous side and how much it traveled on the current side.
                    else
                    {
                        current_latitude = (distance_between_points - (maxLng - prev_distance)) + minLat;
                        current_longitude = maxLng;
                        point2.latitude = current_latitude;
                        point2.longitude = current_longitude;

                    }*/
                }
                
                    //this is the second side of the square that the function travels through.
                    //this side will always have a longitude value of maxLng.
                    //it finds the first point on the perimeter and then adds the distance_between_points
                    //to find the second.
                    else if (current_distance >= (perimeter / 4) && current_distance < (perimeter / 2))
                    {
                        current_longitude = maxLng;
                        current_latitude = minLat + (current_distance - (perimeter / 4));
                        point1.latitude = current_latitude;
                        point1.longitude = current_longitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;

                        point2 = Find_Second_Point_2ndSide(current_distance, distance_between_points, minLat, maxLng, maxLat, perimeter);
                        //when finding the second point, the function needs to know if the current_distance is still on the same
                        //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
                        //it moved to another side.
                        /*
                        if (current_distance <= (perimeter / 2))
                        {
                            current_latitude = minLat + (current_distance - (perimeter / 4));
                            // Console.WriteLine("winning current_latitude = {0}", current_latitude);
                            point2.latitude = current_latitude;
                            point2.longitude = maxLng;
                        }
                        //Now this time, if it moved to the next side, the latitude stays at maxLat. However, the longitude has to change
                        //based off how much distance it traveled on the previous side and how much it traveled on the current side.
                        else
                        {
                            current_longitude = maxLng - (distance_between_points - (maxLat - (prev_distance - (perimeter / 4))));
                            point2.longitude = current_longitude;
                            point2.latitude = maxLat;
                            //Console.WriteLine("current_longitude {0}", current_longitude);
                        }
                        */

                    }
                    //this is the third side of the square that the function travels through.
                    //this side will always have a latitude value of maxLat.
                    //it finds the first point on the perimeter and then adds the distance_between_points
                    //to find the second.
                    else if (current_distance >= (perimeter / 2) && current_distance < ((perimeter * 3) / 4))
                    {
                        current_latitude = maxLat;
                        current_longitude = maxLng - (current_distance - (perimeter / 2));
                        point1.latitude = current_latitude;
                        point1.longitude = current_longitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;
                        //when finding the second point, the function needs to know if the current_distance is still on the same
                        //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
                        //it moved to another side.
                        point2 = Find_Second_Point_3rdSide(current_distance, distance_between_points, maxLng, maxLat, minLng, perimeter);
                        /*
                        if (current_distance <= ((perimeter * 3) / 4))
                        {
                            current_longitude = maxLng - (current_distance - (perimeter / 2));
                            point2.latitude = maxLat;
                            point2.longitude = current_longitude;

                        }
                        //If it moved to the next side, the longitude stays at minLng. However, the latitude has to change
                        //based off how much distance it traveled on the previous side and how much it traveled on the current side.
                        else
                        {
                            current_latitude = maxLat - (distance_between_points - (maxLng - (prev_distance - (perimeter / 2))));
                            point2.latitude = current_latitude;
                            point2.longitude = minLng;
                        }
                        */
                    }
                    //this is the final side of the square that the function travels through.
                    //this side will always have a latitude value of minLng.
                    //it finds the first point on the perimeter and then adds the distance_between_points
                    //to find the second.
                    //this final one should not worry if it passes to another side.
                    else
                    {
                        current_longitude = minLng;
                        current_latitude = maxLat - (current_distance - ((perimeter * 3) / 4));
                        point1.latitude = current_latitude;
                        point1.longitude = current_longitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;
                        current_latitude = maxLat - (current_distance - ((perimeter * 3) / 4));
                        point2.latitude = current_latitude;
                        point2.longitude = minLng;

                    }

                    //add the points the trianglebounds of slice, and then store each slice as a section in sectionSlices
                    point1.weights = new Hashtable();
                    point2.weights = new Hashtable();
                    slice.triangleBounds.Add(point1);
                    slice.triangleBounds.Add(point2);
                    slice.triangleBounds.Add(center);
                    slice.memberPoints = new List<Point>();
                    sectionSlices.Add(slice);
                    
                  
                }



            }
            else if (numSections == 2)
            {
                sectionSlices = TwoSlices(maxLat, minLat, maxLng, minLng);
            }
            int r = 0;
            foreach (SectionSlice thing in sectionSlices)
            {
                Console.WriteLine("Section {0}", r);
                foreach (Point point in thing.triangleBounds)
                {
                    Console.WriteLine("Longitude: {0}, Latitude: {1}", point.longitude, point.latitude);
                }
                r++;
            }

            return sectionSlices;

        }

        static Point Find_Second_Point_1stSide (double current_distance, double distance_between_points, double minLng, double minLat, double maxLng, 
                                                double perimeter)
        {
            double prev_distance = current_distance;
            
            double current_latitude, current_longitude;
            Point returnpoint;
            returnpoint.weights = new Hashtable();
            //when finding the second point, the function needs to know if the current_distance is still on the same
            //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
            //it moved to another side.
            if (current_distance <= (perimeter / 4))
            {
                current_longitude = minLng + current_distance;
                returnpoint.longitude = current_longitude;
                returnpoint.latitude = minLat;

            }
            //If it moved to the next side, the longitude stays at maxLng. However, the latitude has to change
            //based off how much distance it traveled on the previous side and how much it traveled on the current side.
            else
            {
                current_latitude = (distance_between_points - (maxLng - prev_distance)) + minLat;
                current_longitude = maxLng;
                returnpoint.latitude = current_latitude;
                returnpoint.longitude = current_longitude;

            }

            return returnpoint;
        }

        static Point Find_Second_Point_2ndSide (double current_distance, double distance_between_points, double minLat, double maxLng, double maxLat,
                                                double perimeter)
        {
            double prev_distance = current_distance;
            double current_latitude, current_longitude;
            Point returnpoint;
            returnpoint.weights = new Hashtable();

            //when finding the second point, the function needs to know if the current_distance is still on the same
            //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
            //it moved to another side.
            if (current_distance <= (perimeter / 2))
            {
                current_latitude = minLat + (current_distance - (perimeter / 4));
                // Console.WriteLine("winning current_latitude = {0}", current_latitude);
                returnpoint.latitude = current_latitude;
                returnpoint.longitude = maxLng;
            }
            //Now this time, if it moved to the next side, the latitude stays at maxLat. However, the longitude has to change
            //based off how much distance it traveled on the previous side and how much it traveled on the current side.
            else
            {
                current_longitude = maxLng - (distance_between_points - (maxLat - (prev_distance - (perimeter / 4))));
                returnpoint.longitude = current_longitude;
                returnpoint.latitude = maxLat;
                //Console.WriteLine("current_longitude {0}", current_longitude);
            }

            return returnpoint;

        }


        static Point Find_Second_Point_3rdSide(double current_distance, double distance_between_points, double maxLng, double maxLat, double minLng,
                                               double perimeter)
        {
            double prev_distance = current_distance;
            double current_latitude, current_longitude;
            Point returnpoint;
            returnpoint.weights = new Hashtable();

            //when finding the second point, the function needs to know if the current_distance is still on the same
            //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
            //it moved to another side.
            if (current_distance <= ((perimeter * 3) / 4))
            {
                current_longitude = maxLng - (current_distance - (perimeter / 2));
                returnpoint.latitude = maxLat;
                returnpoint.longitude = current_longitude;

            }
            //If it moved to the next side, the longitude stays at minLng. However, the latitude has to change
            //based off how much distance it traveled on the previous side and how much it traveled on the current side.
            else
            {
                current_latitude = maxLat - (distance_between_points - (maxLng - (prev_distance - (perimeter / 2))));
                returnpoint.latitude = current_latitude;
                returnpoint.longitude = minLng;
            }

            return returnpoint;
        }

        //if there two sections needed to be sliced from the square, then this is a special case and this function just
        //accounts for this special case
        static List<SectionSlice> TwoSlices(double maxLat, double minLat, double maxLng, double minLng)
        {
            List<SectionSlice> sectionSlices = new List<SectionSlice>();
            SectionSlice slice1, slice2;
            Point point1, point2, point3, point4;

            point1.latitude = minLat;
            point1.longitude = minLng;
            point1.weights = new Hashtable();
            point2.latitude = maxLat;
            point2.longitude = maxLng;
            point2.weights = new Hashtable();
            point3.latitude = minLat;
            point3.longitude = maxLng;
            point3.weights = new Hashtable();
            point4.latitude = maxLat;
            point4.longitude = minLng;
            point4.weights = new Hashtable();

            slice1.triangleBounds = new List<Point>();
            slice2.triangleBounds = new List<Point>();
            slice1.memberPoints = new List<Point>();
            slice2.memberPoints = new List<Point>();

            slice1.triangleBounds.Add(point1);
            slice1.triangleBounds.Add(point2);
            slice1.triangleBounds.Add(point3);

            slice2.triangleBounds.Add(point1);
            slice2.triangleBounds.Add(point2);
            slice2.triangleBounds.Add(point4);

            sectionSlices.Add(slice1);
            sectionSlices.Add(slice2);

            return sectionSlices;
        }

        //The code for IsPointInBounds, area, and isInside comes from website
        //https://www.geeksforgeeks.org/check-whether-a-given-point-lies-inside-a-triangle-or-not/

        //checks if a point is within the boundry of a triangle made up of three points and returns true or false.
        //For this to work, I needed to split the latitude and longitude values from each point that makes up the boundry.
        //Then I use a helper function to do the math.
        static bool IsPointInBounds(Point currentpoint, SectionSlice bounds)
        {
            double y = currentpoint.longitude;
            double x = currentpoint.latitude;
            double[] latitudes = new double[3];
            double[] longitudes = new double[3];
            int i = 0;
            foreach (Point edge in bounds.triangleBounds)
            {
                latitudes[i] = edge.latitude;
                longitudes[i] = edge.longitude;
                i++;
            }
            if (isInside(latitudes, longitudes, x, y) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Calculates the area of triangle 
        // formed by (x1, y1) (x2, y2) and (x3, y3) 
        static double area(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return Math.Abs((x1 * (y2 - y3) +
                             x2 * (y3 - y1) +
                             x3 * (y1 - y2)) / 2.0);
        }

        //A function to check whether point P(x, y) lies
        //inside the triangle formed by A(x1, y1),
        //B(x2, y2) and C(x3, y3).
        static bool isInside(double[] latitude, double[] longitude, double x, double y)

        {
            
            double x1 = latitude[0];
            double x2 = latitude[1];
            double x3 = latitude[2];
            double y1 = longitude[0];
            double y2 = longitude[1];
            double y3 = longitude[2];
            //Console.WriteLine("Point P({0}, {1}) lies inside triangle formed by A({2}, {3}), B({4}, {5}), C{6}, {7})", x, y, x1, y1, x2, y2, x3, y3);

            // Calculate area of triangle ABC 
            double A = area(x1, y1, x2, y2, x3, y3);
           

            // Calculate area of triangle PBC
            double A1 = area(x, y, x2, y2, x3, y3);
            

            // Calculate area of triangle PAC
            double A2 = area(x1, y1, x, y, x3, y3);

            // Calculate area of triangle PAB
            double A3 = area(x1, y1, x2, y2, x, y);
      
            //gives a high and low value for A (kind of like a plus or minus sign for error)
            double A_minus = A - (A / 100);
            double A_plus = A + (A / 100);
  
            //Checks if sum of A1, A2, and A3 is in the same range as A.
            //The reason that there is a range is due to the chance
            //that the sum might be slightly off due to the complexity of maintaining a
            //consistant decimal value (accounts for errors with significant figures)
            if (A_minus > (A1 + A2 + A3) || A_plus < (A1 + A2 + A3))
            {
                return false;
            }
            else
            {
                
                return true;
            }
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

        //Takes an array of pointers and stores the information in a hashtable.
        //Each point has a hashtable itself, but the point of this function is to combine
        //those hashtables. That means the values of the same key need to be added together.
        //This is done by finding all of the keys (put inside no_repeats) and then adding all of their
        //associated values into one hashtable.
        static Hashtable getScannerResults(List<point> Points)
        {
            Hashtable pointWeights = new Hashtable();
            List<string> commands = new List<string>();
            List<double> values = new List<double>();
            double value = 0;            
            //stores the hashtable information into parallel lists
            foreach (point p in Points)
            {
                foreach (DictionaryEntry entry in p.weights)
                {
                    commands.Add(entry.Key.ToString());
                    values.Add(Convert.ToDouble(entry.Value));
                }
            }
            //the point of no_repeats is so that no string within 
            //the list is the same
            List<string> no_repeats = Get_no_repeats(commands);

            //runs a loop through each string in no_repeats and sees if it 
            //is the same as a string in commands. If it is, since it is parallel
            //to values, it adds the values to a temporary double, which is then added
            //to the hashtable pointWeights.
            foreach (string elem in no_repeats)
            {
                for (int j = 0; j < commands.Count; j++)
                {
                    if (commands[j] == elem)
                    {
                        value += values[j];
                    }
                }

                pointWeights.Add(elem, value);
                value = 0;

            }
            return pointWeights;

        }

        //takes a list of strings and makes a new list where
        //none of the strings repeat (are the same)
        static List<string> Get_no_repeats(List<string> commands)
        {
            List<string> return_array = new List<string>();

            //The bool is set to false for now, but if an element within 
            //commands is equal to an element in return_array, then the bool
            //is changed to true.
            //If the bool is true, then the element in commands is not added
            //to return_array.
            bool a = false;
            foreach (string elem in commands)
            {
                foreach (string word in return_array)
                {
                    if (elem == word)
                    {
                        a = true;
                    }
                }
                if (a == false)
                {
                    return_array.Add(elem);
                }
                else
                {
                    a = false;
                }
            }

            return return_array;
        }

        //helper function that gets the highest weighted total out of a list of weighted totals
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
                section_total_before = weighted_total;
            }
            //Console.WriteLine("Section #{0} has the highest weighted total of {1}", highest_section, highest_weighted_total);
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

            /*double sidewalk_percent = 0;
            weight_percents.Add("sidewalk", sidewalk_percent);
            */

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

            //will eventually have the specific information under "points" (from the json string)
            string weights_string;

            //finds the number of arrays within "sections"
            //this will be helpful in a for loop as I locate each specific value
            int num_sections = sections_array.Count;

            //where the values for each object will be stored
            /*
            int water, hills;
            int sidewalk = 0;
            */
            //there may be multiple json arrays further within "sections" under "points
            //this is used as a temporary json array
            JArray points_array;

            //same use as num_sections, but specific to the amount of elements within "points"
            //there are multiple "points" within sections so I don't know the value yet
            int num_points;

            //temporary point that will transfer points into the 2d array
            point place;

            Dictionary<string, double> dictionary = new Dictionary<string, double>();

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
                    weights_string = results["sections"][i]["points"][j]["weights"].ToString();
                    dictionary = JsonConvert.DeserializeObject<Dictionary<string, double>>(weights_string);
                    place.weights = new Hashtable(dictionary);

                    list.Add(place);

                    /*//by querying the specific values of "water" and "hills" can be found
                    //the JArray objects above were important in finding the values of i and j
                    water = (int)results["sections"][i]["points"][j]["weights"]["water"];
                    hills = (int)results["sections"][i]["points"][j]["weights"]["hills"];

                    //cretes a point with the obtained data and adds it to a list (array)
                    place = new point(water, hills, sidewalk);
                    list.Add(place);
                    */
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

        //takes a list of weighted totals and the jagged list sections and finds the best section
        //and its section number
        static List<Point> findBestSection(List<double> section_totals, List<List<Point>> sections)
        {
            List<Point> best_section = new List<Point>();
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

            int i = 1;
            foreach (List<Point> list in sections)
            {
                if (i == highest_section)
                {
                    return list;

                }
                i++;
            }
            return best_section;  
            }
    }
}
