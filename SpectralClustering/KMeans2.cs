using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace SpectralClustering
{
    class KMeans2
    {
        static int maxIteration = 100;
        public int k;
        public int iteration;
        public List<List<Vector<double>>> clusters;
        public List<Vector<double>> centroids;
        public List<Point> orig_points;
        public List<List<Point>> mapped_clusters;
        public Dictionary<Vector<double>, Point> map;
        public List<Vector<double>> rows;
        public Matrix<double> Y;

        public KMeans2(Matrix<double> Y, List<Point> p, int K)
        {
            this.k = K;
            this.orig_points = p;
            this.Y = Y;
            this.rows = new List<Vector<double>>();
            this.map = new Dictionary<Vector<double>, Point>();
            int index = 0;

            foreach (var v in this.Y.EnumerateRows())
            {
                this.rows.Add(v);
                this.map[v] = this.orig_points[index];
                index++;
            }
            clusters = new List<List<Vector<double>>>();
            centroids = new List<Vector<double>>();
            firstClusters();
        }

        private void firstClusters()
        {
            Vector<double> first = rows[0];
            centroids.Add(first);
            Random rnd = new Random();
            for (int i = 1; i < k; ++i)
            {
                //#1
                double sum = 0;
                foreach (Vector<double> p in rows)
                {
                    double d = nearestCentroid(p);
                    double dx = d * d;
                    sum += dx;
                }
                double rand = rnd.NextDouble() * sum;
                sum = 0;
                foreach (Vector<double> p in rows)
                {
                    double d = nearestCentroid(p);
                    double dx = d * d;
                    sum += dx;
                    if (sum >= rand)
                    {
                        centroids.Add(p);
                        break;
                    }
                }
            }
        }

        private double nearestCentroid(Vector<double> a)
        {
            if (centroids.Count == 0) return 0;
            double min = distance(a, centroids[0]);
            foreach (Vector<double> p in rows)
            {
                double d = distance(a, p);
                if (d < min && p != a)
                {
                    min = d;
                }
            }
            return min;
        }

        private double distance(Vector<double> a, Vector<double> b)
        {
            return MathNet.Numerics.Distance.Euclidean(a,b);
        }

        public void run()
        {

            List<List<List<Vector<double>>>> r = new List<List<List<Vector<double>>>>();
            int i = 0;
            List<Vector<double>> newCentroids = new List<Vector<double>>();
            while (i < maxIteration && previousDistributionChange(newCentroids))
            {
                if (newCentroids.Count != 0) centroids = newCentroids;
                clearClusters();
                foreach (Vector<double> p in rows)
                {
                    int c = findCluster(p);
                    clusters[c].Add(p);
                }
                newCentroids = new List<Vector<double>>();
                foreach (List<Vector<double>> list in clusters)
                {
                    newCentroids.Add(newCentroid(list));
                }
                r.Add(clusters);
                i++;
            }
            iteration = i;
            var last = r.Last();
            mapped_clusters = new List<List<Point>>();
            int curr_index = 0;
            foreach(var v in last)
            {
                mapped_clusters.Add(new List<Point>());
                foreach(var p in v)
                {
                    mapped_clusters[curr_index].Add(map[p]);
                }
                curr_index++;
            }
        }

        private Vector<double> newCentroid(List<Vector<double>> list)
        {
            Vector<double> v = Vector<double>.Build.DenseOfVector(list.First());
            foreach (var v2 in list.Skip(1))
            {
                int index = 0;
                for(index = 0; index < v.Count; index++)
                {
                    v[index] += v2[index];
                }
            }
            v = Vector<double>.Build.Dense(v.Select(x => x / list.Count).ToArray());
            //v = v / list.Count;
            return v;

        }

        private int findCluster(Vector<double> a)
        {
            if (centroids.Count == 0) return 0;
            double min = distance(a, centroids[0]);
            int i = 0;
            int l = 0;
            foreach (Vector<double> p in centroids)
            {
                double d = distance(a, p);
                if (d < min)
                {
                    min = d;
                    l = i;
                }
                i++;
            }
            return l;
        }

        private void clearClusters()
        {
            clusters = new List<List<Vector<double>>>();
            for (int i = 0; i < k; ++i)
            {
                clusters.Add(new List<Vector<double>>());
            }
        }

        private bool previousDistributionChange(List<Vector<double>> list)
        {
            if (list.Count != centroids.Count) return true;
            bool f = false;
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != centroids[i]) return true;
            }
            return f;
        }
    }
}
