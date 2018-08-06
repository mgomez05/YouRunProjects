using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    class create_route
    {
        public struct Point
        {
            public double longitude;
            public double latitude;
            public Hashtable weights;
        }

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
            List<Point> areaPoints = new List<Point>();
            Point center;
            center.longitude = 1.5;
            center.latitude = 1.5;
            center.weights = new Hashtable();
            divideSections(areaPoints, 4, 2, 1, 8, 4, center);

            Console.ReadKey();

        }


        static void divideSections(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {
            List <SectionSlice> sectionSlices = sliceSections_in_four(areaPoints, numSections, maxLat, minLat, maxLng, minLng, center);
            foreach (SectionSlice slice in sectionSlices)
            {
                foreach (Point point in slice.triangleBounds)
                {
                    Console.WriteLine("longitude: {0}, latitude: {1}", point.longitude, point.latitude);
                }
                
            }

        }

        //slices the square given by the max and min coordinates into four triangular sections that contain three points each
        static List<SectionSlice> sliceSections_in_four(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {

            List<SectionSlice> sectionSlices = new List<SectionSlice>();

            //assigns each lat or lng a number starting from 1 to 4
            Dictionary<int, double> corner_numbers = organized_coordinates(maxLat, minLat, maxLng, minLng);
                       
            Point point1;
            Point point2;
            SectionSlice slice;

            //goal is to find three points for each section
            for (int i = 0; i <= numSections; i++)
            {
                //runs a loop through each assigned lat or lng and finds two points to be stored along with the center to create a triangle
                //This is a bit confusing and I honestly could have found a different way, but I wanted to find a unique way to approach this.
                //Essentailly, I assigned numbers (1 to 4) according to the min and max values of the square.
                //Using those numbers, I was able to use a pattern in obtaining the coordinates for the four triangles.
                //It's weird and I'll eventually change it to account for the numSections. But for now I can only do four sections at a 
                //time. Also, I think it might be mathematically impossible to do an odd number of numSections such that the triangles will
                //be equal in area so I'm just putting it out there.

                foreach (KeyValuePair<int, double> elem in corner_numbers)
                {
                    slice.triangleBounds = new List<Point>();
                    slice.triangleBounds.Add(center);

                    if (elem.Key == i)
                    {

                        if (i <= (numSections / 2))
                        {

                            point1.latitude = elem.Value;
                            point1.longitude = minLng;
                            point2.latitude = elem.Value;
                            point2.longitude = maxLng;
                            point1.weights = new Hashtable();
                            point2.weights = new Hashtable();
                            //Console.WriteLine("{0}, {1}", point1.latitude, point1.longitude);
                            //Console.WriteLine("{0}, {1}", point2.latitude, point2.longitude);
                            
                        }
                        else
                        {

                            point1.longitude = elem.Value;
                            point1.latitude = minLat;
                            point2.longitude = elem.Value;
                            point2.latitude = maxLat;
                            point1.weights = new Hashtable();
                            point2.weights = new Hashtable();
                            //Console.WriteLine("{0}, {1}", point1.latitude, point1.longitude);
                           // Console.WriteLine("{0}, {1}", point2.latitude, point2.longitude);
                        }
                        slice.triangleBounds.Add(point1);
                        slice.triangleBounds.Add(point2);
                        slice.memberPoints = new List<Point>();
                        sectionSlices.Add(slice);
                    }
                    
                }
                               
            }

            return sectionSlices;
        }

        static Dictionary<int, double> organized_coordinates(double maxLat, double minLat, double maxLng, double minLng)
        {
            // gives a number value to each coordinate(i.e.maxLat = 1, minLat = 2, maxLng = 3, minLng = 4)
            //does this by placing the coordinates in a dictionary
            double[] coords = new double[] { maxLat, minLat, maxLng, minLng };
            Dictionary<int, double> corner_numbers = new Dictionary<int, double>();

            int j = 0;
            foreach (double coord in coords)
            {
                j++;
                corner_numbers.Add(j, coord);
            }

            return corner_numbers;
        }
    }
}
