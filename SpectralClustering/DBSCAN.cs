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
        Dictionary<Point, List<Point>> EpsNeighborhoods;
        public DBSCAN(List<Point> Points, double Eps=3, double MinPts=3)
        {
            this.Points = Points;
            this.Eps = Eps;
            this.MinPts = MinPts;
            this.clusters = new List<List<Point>>();
            this.EpsNeighborhoods = new Dictionary<Point, List<Point>>();
            PrecalculateEpsNeighborhoods();
        }

        public void PrecalculateEpsNeighborhoods()
        {
            foreach(var p in this.Points)
            {
                List<Point> epsNeighborhood = new List<Point>();
                foreach (var p2 in this.Points)
                {
                    if (p == p2) continue;
                    if (p.dist(p2) <= this.Eps)
                    {
                        epsNeighborhood.Add(p2);
                    }
                }
                this.EpsNeighborhoods.Add(p, epsNeighborhood);
            }
        }

        public void Run()
        {

            if (this.Points == null) return;
            this.Eps *= this.Eps;
            foreach (var p in this.Points)
            {
                if(!p.visited)
                {
                    p.visited = true;
                    var NeighborPts = EpsNeighborhood(p);
                    if (NeighborPts.Count < this.MinPts)
                    {
                        p.noise = true;
                    }
                    else
                    {
                        List<Point> cluster = expandCluster(p);
                        clusters.Add(cluster);
                    }
                }
                
            }

            //clusters = this.Points.Where(x => x.clusterId >= 1).GroupBy(x => x.clusterId).Select(grp => grp.ToList()).ToList();

        }

        public List<Point> expandCluster(Point p)
        {
            List<Point> cluster = new List<Point>();
            List<Point> neighborPts = EpsNeighborhood(p);
            cluster.Add(p);
            p.clustered = true;
            while (neighborPts.Count > 0)
            {
                Point curr = neighborPts[0];
                List<Point> neighborPts2 = EpsNeighborhood(curr);
                if (!curr.visited)
                {
                    curr.visited = true;
                    
                    if(neighborPts2.Count >= MinPts)
                    {
                        foreach(var p3 in neighborPts2)
                        {
                            neighborPts.Add(p3);
                        }
                    }
                }
                if(!curr.clustered)
                {
                    cluster.Add(curr);
                    curr.clustered = true;
                }
                neighborPts.Remove(curr);
            }
            return cluster;
        }

        public List<Point> EpsNeighborhood(Point p)
        {
            return this.EpsNeighborhoods[p];
        }

        public bool AlreadyClustered(Point p)
        {
            foreach(var cluster in this.clusters)
            {
                if (cluster.Contains(p)) return true;
            }
            return false;
        }
    }
}

