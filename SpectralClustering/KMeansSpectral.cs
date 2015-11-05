using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

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
            var result = MathNet.Numerics.Distance.Euclidean(v, centroid);
            return result;
        }

        public int minDistanceToClusters(Vector<double> v)
        {
            var d = new Dictionary<int, double>();
            var clusters = vectorToCluster.Select(x => x.Value).Distinct().ToList();
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

        public void InitialCentroids()
        {
            Random r = new Random(1);
            var randoms = rows.OrderBy(x => r.Next()).Distinct().Take(K).ToList();
            int j = 0;
            foreach (var v in randoms)
            {
                vectorToCluster.Add(v, j);
                this.centroids.Add(v);
            }

            #region otherinitialization
            /*int initial = r.Next(0, rows.Count);
            this.centroids.Add(rows[initial]);
            var distances = new Dictionary<Vector<double>, double>();
            int n_local_trials = 2 + Convert.ToInt32(Math.Log(this.K));
            for (int i = 0; i < rows.Count; i++)
            {
                var curr = rows[i];
                distances[curr] = Math.Pow(MathNet.Numerics.Distance.Euclidean(curr, this.centroids[0]), 2);
            }
            double current_pot = distances.Select(x => x.Value).Sum();

            for(int i = 1; i < this.K; i++)
            {
                List<double> rand_vals = new List<double>();
                while(rand_vals.Count < n_local_trials)
                {
                    double val = r.NextDouble();
                    if(!(val == 1f))
                    {
                        rand_vals.Add(val * current_pot);
                    }
                    
                }
                List<double> cumsum = new List<double>();
                double curr_sum = 0;
                foreach(var v in distances.Values)
                {
                    curr_sum += v;
                    cumsum.Add(curr_sum);
                }

                List<int> candidates = new List<int>();
                List<Vector<double>> candidates_vectors = new List<Vector<double>>();
                foreach(var v in rand_vals)
                {
                    var res = cumsum.BinarySearch(v);
                    if(res < 0)
                    {
                        res = ~res;
                    }
                    candidates.Add(res);
                    candidates_vectors.Add(rows[res]);
                }

                var candidate = candidates_vectors.OrderBy(x => r.Next()).First();
                centroids.Add(candidate);
                distances = new Dictionary<Vector<double>, double>();
                for (int m = 0; m < rows.Count; m++)
                {
                    var curr = rows[m];
                    distances[curr] = Math.Pow(MathNet.Numerics.Distance.Euclidean(curr, candidate), 2);
                }
                current_pot = distances.Select(x => x.Value).Sum();
            }*/
            #endregion
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

            InitialCentroids();

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
                List<int> mindists = new List<int>();
                foreach(Vector<double> v in this.rows)
                {
                    int minDist = minDistanceToClusters(v);
                    mindists.Add(minDist);
                    if(minDist != vectorToCluster[v])
                    {
                        changed = true;
                        vectorToCluster[v] = minDist;
                    }
                    
                }
                var bla = mindists.GroupBy(x => x).Select(item => new { Number = item.Key, Total = item.Count() }).ToList();
                mindists = new List<int>();
                var blah = vectorToCluster.Select(x => x.Value).GroupBy(i => i).Select(item => new { Number = item.Key, Total = item.Count() }).ToList();
                this.iteration++;
            }
            while (changed == true && this.iteration <= 100);
            this.pointClusters = vectorToCluster.GroupBy(x => x.Value).Select(grp => grp.Select(x => map[x.Key]).ToList()).ToList();
            //this.clusters = vectorToCluster.GroupBy(x => x.Value).Select(grp => grp.Select(x => x.Key).ToList()).ToList();
            
        }
    }
}
