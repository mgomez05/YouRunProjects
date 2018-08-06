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
        struct Point
        {
            public double longitdue;
            public double latitude;
            public Hashtable weights;
        }

        struct SectionSlice
        {
            List<Point> triangleBounds;
            List<Point> memberPoints;
        }

    }
}