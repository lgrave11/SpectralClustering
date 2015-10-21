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
        //public Matrix<double> A;
        int maxX;
        int maxY;
        public List<List<Point>> clusterLists = new List<List<Point>>();
        public List<Point> means = new List<Point>();
        public List<Point> meansNew = new List<Point>();
        bool whilecondition;

        public void Cluster()
        {
            double minDist, tempDist;
            Point tempPoint = new Point(0, 0);
            int index = 0;

            

        }

        public KMeans(List<Point> points, int K)
        {
            this.points = points;
            this.K = K;
            this.clusters = new List<List<Point>>();
            maxX = points.Select(p => (int)p.x).Max();
            maxY = points.Select(p => (int)p.y).Max();

            Random r = new Random();

            for (int i = 0; i < K; i++)
            {
                clusters.Add(new List<Point>());
                var mean = new Point(r.Next(0, maxX), r.Next(0, maxY));
                means.Add(mean);
            }

            double minDist, tempDist;
            Point tempPoint = new Point(0, 0);
            int index = 0;

            do
            {
                whilecondition = true; //condition set to true for while loop to run, and will be checked during the loop, to check if another iteration is neccesary

                /*Lists is getting cleared for new calculation of cluster*/
                foreach (List<Point> someList in clusters)
                {
                    someList.Clear();
                }
                meansNew.Clear();
                means.ForEach((item) => { meansNew.Add(new Point(item.x, item.y)); });

                /*Points gets assigned to clusterLists to the clusterPoint with lowest distance*/
                foreach (Point inpPoint in points)
                {
                    minDist = double.MaxValue;
                    foreach (Point clustPoint in means)
                    {
                        tempDist = inpPoint.dist(clustPoint);

                        if (tempDist < minDist)
                        {
                            minDist = tempDist;
                            index = means.IndexOf(clustPoint);
                        }
                    }
                    clusters[index].Add(inpPoint);
                }

                /*Centroid of the clusterLists gets calculated and a new clusterPoint coordinate gets assigned*/
                foreach (List<Point> averageList in clusters)
                {
                    index = clusters.IndexOf(averageList);
                    if (averageList.Count > 0)
                    {
                        means[index].x = averageList.Average(a => a.x);
                        means[index].y = averageList.Average(a => a.y);
                    }
                }

                /*Check if any change has occured to the clusterPoints since last iteration,
                 if so do another iteration*/
                foreach (Point point in means)
                {
                    if (point.CompareTo(meansNew[means.IndexOf(point)]) != 0)
                        whilecondition = false;
                }

            } while (!whilecondition);

        }
    }
}
