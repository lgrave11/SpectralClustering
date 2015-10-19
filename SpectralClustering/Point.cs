using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralClustering
{
    public class Point
    {
        public int x;
        public int y;
        public int matrix_x;
        public int matrix_y;
        public double Eigen;
        public double clusternum;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            Eigen = 0f;
            clusternum = -1f;
        }

        public double dist(Point p2)
        {
            return Math.Sqrt(Math.Pow(this.x - p2.x, 2) + Math.Pow(this.y - p2.y, 2));
        }
        public double manhattan_dist(Point p2)
        {
            return Math.Abs(this.x - p2.x) + Math.Abs(this.y - p2.y);
        }
	    
    }
}
