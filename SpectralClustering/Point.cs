using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralClustering
{
    enum Cluster { Unclassified = -1, Noise}
    public class Point : Object
    {
        public double x;
        public double y;
        public int matrix_x;
        public int matrix_y;
        public List<double> Eigen;
        public double clusternum;
        public int clusterId;
        public bool visited;
        public bool noise;
        public bool clustered;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
            Eigen = new List<double>();
            clusternum = -1f;
            visited = false;
            noise = false;
            clustered = false;
            clusterId = -1;
        }

        public double dist(Point p2)
        {
            return Math.Sqrt(Math.Pow(this.x - p2.x, 2) + Math.Pow(this.y - p2.y, 2));
        }
        public double manhattan_dist(Point p2)
        {
            return Math.Abs(this.x - p2.x) + Math.Abs(this.y - p2.y);
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Point p2 = obj as Point;
            if ((Object)p2 == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.x == p2.x) && (this.y == p2.y);
        }

        public int CompareTo(object obj)
        {
            Point p1 = (Point)obj;

            if (p1.x > x)
                return 1;
            else if (p1.x == x)
            {
                if (p1.y > y)
                    return 1;
                else if (p1.y == y)
                    return 0;
                else
                    return -1;
            }
            else
                return -1;
        }

    }
}
