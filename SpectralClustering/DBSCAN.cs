using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralClustering
{
    public class DBSCAN
    {
        public List<List<Point>> clusters;
        double Eps;
        double MinPts;
        List<Point> Points;
        int C = 0;
        public DBSCAN(List<Point> Points, double Eps, double MinPts)
        {
            this.Points = Points;
            this.Eps = Eps;
            this.MinPts = MinPts;
            this.clusters = new List<List<Point>>();
        }

        public void Run()
        {
            if (this.Points == null) return;
            this.Eps *= this.Eps;
            foreach (var p in this.Points)
            {
                p.visited = true;
                if(!(p.clusterId >= 1))
                {
                    expandCluster(p);
                    C++;
                }
            }
            clusters = this.Points.Where(x => x.clusterId >= 1).GroupBy(x => x.clusterId).Select(grp => grp.ToList()).ToList();
        }

        public bool expandCluster(Point p)
        {
            List<Point> neighborPts = regionQuery(p);
            if(neighborPts.Count < this.Eps)
            {
                p.clusterId = 0; // Noise.
                return true;
            }
            else
            {
                p.clusterId = C;
                foreach(var np in neighborPts) np.clusterId = C;
                while(neighborPts.Count > 0)
                {
                    Point curr = neighborPts[0];
                    List<Point> neighborPts2 = regionQuery(curr);
                    if (neighborPts2.Count >= this.MinPts)
                    {
                        foreach(var p3 in neighborPts2)
                        {
                            if(p3.clusterId == -1 || p3.clusterId == 0)
                            {
                                if (p3.clusterId == -1) neighborPts.Add(p3);
                                p3.clusterId = C;
                            }
                        }
                    }
                    neighborPts.Remove(curr);
                }
                return true;
            }
        }

        public List<Point> regionQuery(Point p)
        {
            List<Point> epsNeighborhood = new List<Point>();
            foreach (var p2 in this.Points)
            {
                if (p == p2) continue;
                if(p.dist(p2) <= this.Eps) {
                    epsNeighborhood.Add(p2);
                }
            }
            return epsNeighborhood;
        }
    }

    
}
