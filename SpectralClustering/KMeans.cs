using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace SpectralClustering
{

    public class KMeans
    {
        public List<List<Point>> clusters;
        public List<Point> points;
        public int K;
        public int iteration = 0;
        public List<Point> centroids = new List<Point>();
        public Dictionary<Point, int> pointToCluster;

        public double distanceToCluster(Point p, Point centroid)
        {
            return p.dist(centroid);
        }

        public int minDistanceToClusters(Point p)
        {
            var d = new Dictionary<int, double>();
            for (int i = 0; i < this.K; i++)
            {
                d.Add(i, distanceToCluster(p, this.centroids[i]));
            }
            int mindist = d.OrderBy(x => x.Value).Select(x => x.Key).ToList()[0];
            return mindist;
        }

        public void updateCentroids()
        {
            for(int i = 0; i < K; i++)
            {
                var clus = pointToCluster.Where(x => x.Value == i).Select(x => x.Key).ToList();
                this.centroids[i] = new Point(clus.Average(a => a.x), clus.Average(a => a.y));
            }
        }

        public KMeans(List<Point> points, int K)
        {
            this.points = points;
            this.K = K;
            this.pointToCluster = new Dictionary<Point, int>();

            Random r = new Random();
            var randoms = points.OrderBy(x => r.Next()).Take(K).ToList();
            for (int i = 0; i < K; i++)
            {
                pointToCluster.Add(randoms[i], i);
                this.centroids.Add(randoms[i]);
            }

            foreach(var p in this.points)
            {
                int mindist = minDistanceToClusters(p);
                if(!pointToCluster.ContainsKey(p))
                {
                    pointToCluster.Add(p, mindist);
                }
                
            }

            bool changed = false;
            do
            {
                changed = false;
                updateCentroids();
                foreach(Point p in this.points)
                {
                    int minDist = minDistanceToClusters(p);
                    if(minDist != pointToCluster[p])
                    {
                        changed = true;
                        pointToCluster[p] = minDist;
                    }

                }
                this.iteration++;
            }
            while (changed == true && this.iteration <= 100);
            this.clusters = pointToCluster.GroupBy(x => x.Value).Select(grp => grp.Select(x => x.Key).ToList()).ToList();

        }
    }
}
