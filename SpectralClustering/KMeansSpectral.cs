using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace SpectralClustering
{

    public class KMeansSpectral
    {
        List<Vector<double>> rows;
        public Matrix<double> Y;
        public List<List<Vector<double>>> clusters;
        public List<List<Point>> pointClusters;
        public Dictionary<Vector<double>, Point> map;
        public List<Point> points;
        public int K;
        public int iteration = 0;
        public List<Vector<double>> centroids = new List<Vector<double>>();
        public Dictionary<Vector<double>, int> vectorToCluster;

        public double distanceToCluster(Vector<double> v, Vector<double> centroid)
        {
            return MathNet.Numerics.Distance.Euclidean(v, centroid);
        }

        public int minDistanceToClusters(Vector<double> v)
        {
            var d = new Dictionary<int, double>();
            for (int i = 0; i < this.K; i++)
            {
                d.Add(i, distanceToCluster(v, this.centroids[i]));
            }
            int mindist = d.OrderBy(x => x.Value).Select(x => x.Key).ToList()[0];
            return mindist;
        }

        public void updateCentroids()
        {
            for(int i = 0; i < K; i++)
            {
                var clus = vectorToCluster.Where(x => x.Value == i).Select(x => x.Key).ToList();
                Vector<double> v = clus.First();
                foreach(var v2 in clus.Skip(1))
                {
                    v.Add(v2);
                }
                v = v / clus.Count;
                this.centroids[i] = v;
            }
        }

        public KMeansSpectral(Matrix<double> Y, List<Point> points, int K)
        {
            this.Y = Y;
            this.points = points;
            this.rows = new List<Vector<double>>();
            this.map = new Dictionary<Vector<double>, Point>();
            int index = 0;
            foreach (var v in this.Y.EnumerateRows())
            {
                this.rows.Add(v);
                this.map[v] = this.points[index++];
            }
            

            this.K = K;
            this.vectorToCluster = new Dictionary<Vector<double>, int>();

            Random r = new Random();
            var randoms = rows.OrderBy(x => r.Next()).Take(K).ToList();
            for (int i = 0; i < K; i++)
            {
                vectorToCluster.Add(randoms[i], i);
                this.centroids.Add(randoms[i]);
            }

            foreach(var p in this.rows)
            {
                int mindist = minDistanceToClusters(p);
                if(!vectorToCluster.ContainsKey(p))
                {
                    vectorToCluster.Add(p, mindist);
                }
                
            }

            bool changed = false;
            do
            {
                changed = false;
                updateCentroids();
                List<List<Vector<double>>> newClusters = new List<List<Vector<double>>>();
                for(int i = 0; i < this.K; i++)
                {
                    newClusters.Add(new List<Vector<double>>());
                    foreach(Vector<double> v in this.rows)
                    {
                        int minDist = minDistanceToClusters(v);
                        if(minDist != i)
                        {
                            changed = true;
                            vectorToCluster[v] = minDist;
                        }
                        else
                        {
                            vectorToCluster[v] = i;
                        }

                    }
                }
                this.iteration++;
                this.clusters = newClusters;
            }
            while (changed == true && this.iteration <= 100);
            this.pointClusters = new List<List<Point>>();
            this.pointClusters = vectorToCluster.GroupBy(x => x.Value).Select(grp => grp.Select(x => map[x.Key]).ToList()).ToList();
            //this.clusters = vectorToCluster.GroupBy(x => x.Value).Select(grp => grp.Select(x => x.Key).ToList()).ToList();
            
        }
    }
}
