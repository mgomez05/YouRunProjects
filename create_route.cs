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
            center.longitude = 8;
            center.latitude = 8;
            center.weights = new Hashtable();
            divideSections(areaPoints, 6, 32, 16, 32, 16, center);

            Console.ReadKey();

        }


        static void divideSections(List<Point> areaPoints, int numSections, double maxLat, double minLat, double maxLng,
                                         double minLng, Point center)
        {
            int i = 1;
            List<SectionSlice> sectionSlices = SlicetheSquarePie(areaPoints, numSections, maxLat, minLat, maxLng, minLng, center);
            foreach (SectionSlice slice in sectionSlices)
            {
                Console.WriteLine("Section #{0}", i);
                foreach (Point point in slice.triangleBounds)
                {
                    Console.WriteLine("longitude: {0}, latitude: {1}", point.longitude, point.latitude);
                }
                i++;
            }

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

    }
}