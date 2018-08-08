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
            Point point1;
            point1.longitude = 6;
            point1.latitude = 1;
            point1.weights = new Hashtable();
            areaPoints.Add(point1);
            Point center;
            center.longitude = 4;
            center.latitude = 4;
            center.weights = new Hashtable();
            List<List<Point>> sections = new List<List<Point>>();
            sections = divideSections(areaPoints, 4, 8, 0, 8, 0, center);

            Console.ReadKey();

        }


        static List<List<Point>> divideSections(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {
            int j = 1;
            int i = 1;
            int z = 1;
            List<SectionSlice> sectionSlices = SlicetheSquarePie(areaPoints, numSections, maxLat, minLat, maxLng, minLng, center);

            foreach (Point point in areaPoints)
            {
                Point currentPoint = point;

                foreach (SectionSlice slice in sectionSlices)
                {                    
                    if (IsPointInBounds(currentPoint, slice) == true)
                    {
                        slice.memberPoints.Add(currentPoint);
                        Console.WriteLine("Point {1} is within bounds of section {0}", i, z);
                        break;
                    }
                    i++;
                }

                if (i > numSections)
                {
                    Console.WriteLine("point is not within selected area");
                    areaPoints.Remove(currentPoint);
                    break;

                }
                z++;
            }

            List<List<Point>> sections = new List<List<Point>>();

            foreach (SectionSlice slice in sectionSlices)
            {
                sections.Add(slice.memberPoints);
            }

            return sections;

            /*foreach (SectionSlice slice in sectionSlices)
            {
                Console.WriteLine("Section #{0}", j);
                foreach (Point point in slice.triangleBounds)
                {
                    Console.WriteLine("longitude: {0}, latitude: {1}", point.longitude, point.latitude);
                }
                j++;
            }
            */


        }

        //slices the square given by the max and min coordinates into four triangular sections that contain three points each
        static List<SectionSlice> SlicetheSquarePie(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {

            List<SectionSlice> sectionSlices = new List<SectionSlice>();

            //assigns each lat or lng a number starting from 1 to 4
            //Dictionary<int, double> corner_numbers = organized_coordinates(maxLat, minLat, maxLng, minLng);

            Point point1;
            Point point2;
            SectionSlice slice;
            double side_length = maxLat - minLat;
            double perimeter = side_length * 4;
            double distance_between_points = perimeter / numSections;
            double current_distance = 0;
            double prev_distance;
            double current_latitude = 0;
            double current_longitude = 0;

            if (numSections > 3)
            {

                for (int i = 0; i < numSections; i++)
                {
                    slice.triangleBounds = new List<Point>();
                    if (current_distance <= (perimeter / 4))
                    {
                        current_longitude = minLng + current_distance;
                        current_latitude = minLat;
                        point1.longitude = current_longitude;
                        point1.latitude = current_latitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;
                        if (current_distance <= (perimeter / 4))
                        {
                            current_longitude = minLng + current_distance;
                            point2.longitude = current_longitude;
                            point2.latitude = minLat;
                        }
                        else
                        {
                            current_latitude = (distance_between_points - (maxLng - prev_distance)) + minLat;
                            current_longitude = maxLng;
                            point2.latitude = current_latitude;
                            point2.longitude = current_longitude;
                        }
                    }
                    else if (current_distance > (perimeter / 4) && current_distance <= (perimeter / 2))
                    {
                        current_longitude = maxLng;
                        current_latitude = minLat + (current_distance - (perimeter / 4));
                        point1.latitude = current_latitude;
                        point1.longitude = current_longitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;
                        if (current_distance <= (perimeter / 2))
                        {
                            current_latitude = minLat + (current_distance - (perimeter / 4));
                            point2.latitude = current_latitude;
                            point2.longitude = maxLng;
                        }
                        else
                        {
                            current_longitude = maxLng - (distance_between_points - (maxLat - (prev_distance - (perimeter / 4))));
                            point2.longitude = current_longitude;
                            point2.latitude = maxLat;
                        }

                    }
                    else if (current_distance > (perimeter / 2) && current_distance <= ((perimeter * 3) / 4))
                    {
                        current_latitude = maxLat;
                        current_longitude = maxLng - (current_distance - (perimeter / 2));
                        point1.latitude = current_latitude;
                        point1.longitude = current_longitude;
                        prev_distance = current_distance;
                        current_distance = current_distance + distance_between_points;
                        if (current_distance <= ((perimeter * 3) / 4))
                        {
                            current_longitude = maxLng - (current_distance - (perimeter / 4));
                            point2.latitude = current_latitude;
                            point2.longitude = maxLng;
                        }
                        else
                        {
                            current_latitude = maxLat - (distance_between_points - (maxLng - (prev_distance - (perimeter / 2))));
                            point2.latitude = current_latitude;
                            point2.longitude = minLng;
                        }
                    }
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

                    point1.weights = new Hashtable();
                    point2.weights = new Hashtable();
                    slice.triangleBounds.Add(point1);
                    slice.triangleBounds.Add(point2);
                    slice.triangleBounds.Add(center);
                    slice.memberPoints = new List<Point>();
                    sectionSlices.Add(slice);

                }



            }
            return sectionSlices;

        }
        static bool IsPointInBounds(Point currentpoint, SectionSlice bounds)
        {
            double y = currentpoint.latitude;
            double x = currentpoint.longitude;
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
        /* A utility function to calculate area of triangle 
   formed by (x1, y1) (x2, y2) and (x3, y3) */
        static double area(double x1, double y1, double x2,
                           double y2, double x3, double y3)
        {
            return Math.Abs((x1 * (y2 - y3) +
                             x2 * (y3 - y1) +
                             x3 * (y1 - y2)) / 2.0);
        }

        /* A function to check whether point P(x, y) lies
        inside the triangle formed by A(x1, y1),
        B(x2, y2) and C(x3, y3) */
        static bool isInside(double[] latitude, double[] longitude, double x, double y)

        {

            double x1 = latitude[0];
            double x2 = latitude[1];
            double x3 = latitude[2];
            double y1 = longitude[0];
            double y2 = longitude[1];
            double y3 = longitude[2];


            /* Calculate area of triangle ABC */
            double A = area(x1, y1, x2, y2, x3, y3);

            /* Calculate area of triangle PBC */
            double A1 = area(x, y, x2, y2, x3, y3);

            /* Calculate area of triangle PAC */
            double A2 = area(x1, y1, x, y, x3, y3);

            /* Calculate area of triangle PAB */
            double A3 = area(x1, y1, x2, y2, x, y);

            /* Check if sum of A1, A2 and A3 is same as A */
            return (A == A1 + A2 + A3);
        }

        /* Driver program to test above function */



    }
}
