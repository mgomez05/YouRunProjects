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

        public class JsonInfo
        {
            public Dictionary<string, double> weight_percentages { get; set; }

        }


        static void Main(string[] args)
        {
            //just test data
            List<Point> areaPoints = createTestPointsforMain();
            Point center;
            center.longitude = 4;
            center.latitude = 4;
            center.weights = new Hashtable();

            string jsonString = "{ \"preferences\": [{\"name\": \"hills\", \"weight\": .75},{ \"name\": \"water\", \"weight\": .55}, { \"name\": \"grass\", \"weight\": -.44}], \"current location\": { \"lat\": 0.99, \"lng\": 3.99}, \"radius\": 20}";

            findBestRunningArea(jsonString);

            List<List<Point>> sections = new List<List<Point>>();
            //sections = divideSections(areaPoints, 4, 8, 0, 8, 0, center);


            Console.ReadKey();

        }

        static void findBestRunningArea(string jsonString)
        {

            Hashtable preferences = Get_Preferences(jsonString);

            double radius = Get_Radius(jsonString);

            Point currentLocation = Get_currentLocation(jsonString);

            List<double> boundries = Get_squareBoundary(currentLocation, radius);

            List<Point> areaPoints = HTTP_Get_Points(180, 0, 180, 0);

            /*foreach (Point point in areaPoints)
            {
                Console.WriteLine("areaPoints    Latitude: {0}, Longitude: {1}", point.latitude, point.longitude);
            }
            */
                

            List<List<Point>> sections = divideSections(areaPoints, 4, boundries[0], boundries[1], boundries[2], boundries[3], currentLocation);

            int i = 1; 
            foreach (List<Point> list in sections)
            {
                foreach (Point point in list)
                {
                    //Console.WriteLine("This point is in section {2}, Latitude: {0}, Longitude: {1}", point.latitude, point.longitude, i);
                    i++;
                }
            }
     
            string new_jsonString = create_JSON_string(preferences, sections);

        }
               
           
        static string create_JSON_string(Hashtable preferences, List<List<Point>> sections)
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();
            Write_Weight_Percentages(preferences, writer);
            Write_Sections(sections, writer);
            writer.WriteEndObject();

            JObject o = (JObject)writer.Token;

            string json = o.ToString();

            return json;

        }
        static void Write_Weight_Percentages(Hashtable preferences, JTokenWriter writer)
        {               
            writer.WritePropertyName("weight_percentages");
            writer.WriteStartArray();
            foreach (DictionaryEntry entry in preferences)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(entry.Key.ToString());
                writer.WriteValue(Convert.ToDouble(entry.Value));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();               
        }

        static void Write_Sections(List<List<Point>> sections, JTokenWriter writer)
        {
            writer.WritePropertyName("sections");
            writer.WriteStartArray();

            int id_number = 1;
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
                    foreach (DictionaryEntry hash in point.weights)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName(hash.Key.ToString());
                        writer.WriteValue(Convert.ToDouble(hash.Value));
                        writer.WriteEndObject();
                    }
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

            string preferences_string = results["preferences"].ToString();

            JArray preferences = JArray.Parse(preferences_string);

            int num_preferences = preferences.Count;

            Hashtable returnHashtable = new Hashtable();

            for (int i = 0; i < num_preferences; i++)
            {
                returnHashtable.Add(preferences[i]["name"], preferences[i]["weight"]);
               
            }

            return returnHashtable;
        }

        static double Get_Radius(string jsonString)
        {
            JObject results = JObject.Parse(jsonString);

            double radius = Convert.ToDouble(results["radius"]);

            return radius;
        }

        static Point Get_currentLocation(string jsonString)
        {
            JObject results = JObject.Parse(jsonString);

            Point returnLocation;

            returnLocation.latitude = Convert.ToDouble(results["current location"]["lat"]);
            returnLocation.longitude = Convert.ToDouble(results["current location"]["lng"]);
                        
            returnLocation.weights = new Hashtable();

            return returnLocation;

        }

        static List<double> Get_squareBoundary(Point currentLocation, double radius)
        {
            double earth_radius = 3960.0;
            double degrees_to_radians = Math.PI / 180.0;
            double radians_to_degrees = 180.0 / Math.PI;

            double change_in_latitude = (radius / earth_radius) * radians_to_degrees;
            double r = earth_radius * Math.Cos(currentLocation.latitude * degrees_to_radians);
            double change_in_longitude = (radius / r) * radians_to_degrees;

            //Console.WriteLine("change in latitude: {0}, change in longitude: {1}", change_in_latitude, change_in_longitude);

            double maxLat = currentLocation.latitude + change_in_latitude;
            double minLat = currentLocation.latitude - change_in_latitude;
            double maxLng = currentLocation.longitude + change_in_longitude;
            double minLng = currentLocation.longitude - change_in_longitude;

            //Console.WriteLine("maxLat: {0}, minLat {1}, maxLng {2}, minLng {3}", maxLat, minLat, maxLng, minLng);

            List<double> returnBoundaries = new List<double>();

            returnBoundaries.Add(maxLat);
            returnBoundaries.Add(minLat);
            returnBoundaries.Add(maxLng);
            returnBoundaries.Add(minLng);

            foreach (double boundry in returnBoundaries)
            {
                Console.WriteLine(boundry);
            }

            return returnBoundaries;


        }

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
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string runnerAreaJson = reader.ReadToEnd();
            // Display the content.  
            Console.WriteLine(runnerAreaJson);
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            List<Point> areaPoints = Parse_runnerAreaJson(runnerAreaJson);

           
            return areaPoints;
        }

        static List<Point> Parse_runnerAreaJson(string runnerAreaJson)
        {
            JObject results = JObject.Parse(runnerAreaJson);

            string results_string = results["results"].ToString();

            JArray json_points_array = (JArray)results["results"];

            int num_points = json_points_array.Count;

            JArray weights_array;

            int num_weights;

            Point temp;
            List<Point> areaPoints = new List<Point>();

            for (int i = 0; i < num_points; i++)
            {
                temp.weights = new Hashtable();
                temp.latitude = Convert.ToDouble(results["results"][i]["latitude"]);
                temp.longitude = Convert.ToDouble(results["results"][i]["longitude"]);
                temp.weights = new Hashtable();
                weights_array = (JArray)results["results"][i]["weights"];
                num_weights = weights_array.Count;
                Console.WriteLine("{0}", num_weights);
                
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

            foreach (SectionSlice slice in sectionSlices)
            {
                Console.WriteLine("Section #{0}", j);
                foreach (Point point in slice.triangleBounds)
                {
                    Console.WriteLine("longitude: {0}, latitude: {1}", point.longitude, point.latitude);
                }
                j++;
            }
            
            
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
                        //when finding the second point, the function needs to know if the current_distance is still on the same
                        //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
                        //it moved to another side.
                        if (current_distance <= (perimeter / 4))
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
                           
                        }
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
                        

                        //when finding the second point, the function needs to know if the current_distance is still on the same
                        //side or if it passed onto the next. The if statement is if it is on the same side. The else statement is if
                        //it moved to another side.
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
            return sectionSlices;

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


       
            // Check if sum of A1, A2 and A3 is same as A 
            double A_minus = A - (A / 50);
            double A_plus = A + (A / 50);

            


            Console.WriteLine("A minus {0} < {1} < A plus {2}", A_minus, A1 + A2 + A3, A_plus);

        
            if (A_minus > (A1 + A2 + A3) || A_plus < (A1 + A2 + A3))
            {
                return false;
            }
            else
            {
                
                return true;
            }
        }



        //12 points to help test
        //I just didn't want Main to be too long
        static List<Point> createTestPointsforMain()
        {
            List<Point> areaPoints = new List<Point>();
            Point point1, point2, point3, point4, point5, point6, point7, point8, point9, point10, point11, point12;
            point1.longitude = 1;
            point1.latitude = 0;
            point1.weights = new Hashtable();
            areaPoints.Add(point1);
            point2.longitude = 6;
            point2.latitude = 1;
            point2.weights = new Hashtable();
            areaPoints.Add(point2);
            point3.longitude = 6;
            point3.latitude = 1;
            point3.weights = new Hashtable();
            areaPoints.Add(point3);
            point4.longitude = 5;
            point4.latitude = 4;
            point4.weights = new Hashtable();
            areaPoints.Add(point4);
            point5.longitude = 6;
            point5.latitude = 4;
            point5.weights = new Hashtable();
            areaPoints.Add(point5);
            point6.longitude = 7;
            point6.latitude = 4;
            point6.weights = new Hashtable();
            areaPoints.Add(point6);
            point7.longitude = 4;
            point7.latitude = 5;
            point7.weights = new Hashtable();
            areaPoints.Add(point7);
            point8.longitude = 40;
            point8.latitude = 60;
            point8.weights = new Hashtable();
            areaPoints.Add(point8);
            point9.longitude = 4;
            point9.latitude = 7;
            point9.weights = new Hashtable();
            areaPoints.Add(point9);
            point10.longitude = 3;
            point10.latitude = 4;
            point10.weights = new Hashtable();
            areaPoints.Add(point10);
            point11.longitude = 2;
            point11.latitude = 4;
            point11.weights = new Hashtable();
            areaPoints.Add(point11);
            point12.longitude = 4;
            point12.latitude = 5;
            point12.weights = new Hashtable();
            areaPoints.Add(point12);

            return areaPoints;
        }

    


    }
}
