using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.IO;

namespace SpectralClustering
{
    public class SpectralClustering
    {
        List<Point> points;
        public List<List<Point>> clusters;
        int maxClusters;
        Func<Point, Point, Double> similarityMeasure;
        Evd<double> evd;
        int k;
        bool useKMeans;
        public SpectralClustering(List<Point> lp, Func<Point, Point, double> similarityMeasure, int maxClusters = 10, int k=10, bool useKMeans=false)
        {
            this.points = lp;
            this.similarityMeasure = similarityMeasure;
            this.maxClusters = maxClusters;
            this.k = k;
            this.useKMeans = useKMeans;
            this.clusters = new List<List<Point>>();
        }

        public void Run()
        {
            List<Point> eigenDecomposed = EigenDecomposition();
            if(useKMeans)
            {
                Matrix<double> eigenVectorMatrix;
                List<Tuple<Double, Vector<double>>> eigenVectors = new List<Tuple<double, Vector<double>>>();
                List<double> eigenValues = this.evd.EigenValues.Select(x => x.Real).ToList();
                int index = 0;
                foreach (var v in evd.EigenVectors.EnumerateColumns())
                {
                    if (index == 0)
                    {
                        index++;
                        continue;
                    }
                    Tuple<Double, Vector<double>> tmp = new Tuple<double, Vector<double>>(eigenValues[index], v);
                    eigenVectors.Add(tmp);
                    index++;
                }
                var eigenVectors2 = eigenVectors.OrderBy(x => x.Item1).Take(k).ToList();
                index = 0;
                var tmp2 = eigenVectors2.Select(x => x.Item2).ToArray();
                eigenVectorMatrix = Matrix<double>.Build.DenseOfColumnVectors(tmp2);
                // Cluster and shit
                KMeansSpectral kms = new KMeansSpectral(eigenVectorMatrix, this.points, this.k);
                //KMeans2 kms = new KMeans2(eigenVectorMatrix, this.points, this.k);
                clusters.AddRange(kms.pointClusters);
                
            }
            else
            {
                List<List<Point>> cutCommunities = new List<List<Point>>();
                cutCommunities = Cut(eigenDecomposed, maxClusters);
                clusters.AddRange(cutCommunities);
            }
            
        }

        public List<Point> EigenDecomposition()
        {
            //Console.WriteLine("Making matrixes");
            if (this.points.Count == 1) return this.points;
            double[,] S = new double[this.points.Count, this.points.Count];
            //List<List<Double>> S = new List<List<double>>();
            for (int i = 0; i < this.points.Count; i++)
            {
                for (int j = 0; j < this.points.Count; j++)
                {
                    if (i == j)
                    {
                        S[i, j] = 0f;
                    }
                    else
                    {
                        S[i, j] = similarityMeasure(this.points[i], this.points[j]);
                    }
                }
            }
            Matrix<double> A = Matrix<double>.Build.DenseOfArray(S);
            using (TextWriter tw = new StreamWriter("A.txt"))
            {
                for (int j = 0; j < this.points.Count; j++)
                {
                    for (int i = 0; i < this.points.Count; i++)
                    {
                        tw.Write("{0:0.00} ", A[i, j]);
                    }
                    tw.WriteLine();
                }
            }
            using (TextWriter tw = new StreamWriter("A2.txt"))
            {
                for (int j = 0; j < this.points.Count; j++)
                {
                    for (int i = 0; i < this.points.Count; i++)
                    {
                        tw.Write("{0:0.00} ", S[i, j]);
                    }
                    tw.WriteLine();
                }
            }
            Vector<double> dVector = A.RowAbsoluteSums();
            Matrix<double> D = Matrix<double>.Build.DenseOfDiagonalVector(dVector);
            Matrix<double> I = Matrix<double>.Build.DenseIdentity(D.RowCount, D.ColumnCount);
            Matrix<double> L;
            if (useKMeans)
            {
                L = D - A;
                //L = D.Inverse() * A;
                //Vector<double> dVector2 = Vector<double>.Build.DenseOfEnumerable(dVector.Select(x => Math.Pow(x, -0.5)));
                //Matrix<double> D2 = Matrix<double>.Build.DenseOfDiagonalVector(dVector2);
                //L = I - D2 * A * D2;
            }
            else
            {
                L = D - A;
                //L = D.Inverse() * A;
                //Vector<double> dVector2 = Vector<double>.Build.DenseOfEnumerable(dVector.Select(x => Math.Pow(x, -0.5)));
                //Matrix<double> D2 = Matrix<double>.Build.DenseOfDiagonalVector(dVector2);
                //L = I - D2 * A * D2;
            }

            Evd<double> evd = L.Evd();
            Vector<double> eigenVector = evd.EigenVectors.Column(1);
            List<double> eigenValues = evd.EigenValues.Select(x => x.Real).ToList();

            List<Tuple<Double, Vector<double>>> eigenVectors = new List<Tuple<double, Vector<double>>>();
            int index = 0;
            foreach (var v in evd.EigenVectors.EnumerateColumns())
            {
                if (index == 0)
                {
                    index++;
                    continue;
                }
                Tuple<Double, Vector<double>> tmp = new Tuple<double, Vector<double>>(eigenValues[index], v);
                eigenVectors.Add(tmp);
                index++;
            }
            //var eigenVectors2 = eigenVectors.OrderByDescending(x => x.Item1).Select(x => x.Item2).First();
            var eigenVectors2 = eigenVectors.Skip(1).Select(x => x.Item2).First();
            Chart c = new Chart();
            c.ChartAreas.Add("EigenValues");
            
            c.Series.Add("bla");
            c.Series["bla"].ChartType = SeriesChartType.Point;
            c.Series["bla"].Color = Color.Black;
            double xIndex = 0;
            foreach(var p in eigenVectors2.OrderByDescending(x=> x))
            {
                c.Series["bla"].Points.AddXY(xIndex++, p);
            }
            var fn = String.Format("{0}.png", this.points.Count());
            c.SaveImage(fn, ChartImageFormat.Png);

            this.evd = evd;
            for (int ev = 0; ev < eigenVector.Count; ev++)
            {
                this.points[ev].Eigen.Add(eigenVector[ev]);
                //this.points[ev].Eigen.AddRange(evd.EigenVectors.Row(ev).Skip(1).Take(k));
            }
            var sortedItemList = this.points.OrderBy(x => x.Eigen[0]).ToList();
            
            return sortedItemList;
        }

        public List<List<Point>> Cut(List<Point> sortedItemList, int maxClusters)
        {
            var lgs = LargestGaps(sortedItemList).ToList();
            lgs = lgs.Take(maxClusters - 1).OrderBy(x => x.Item1).ToList();
            lgs.Insert(0, new Tuple<int, double>(0, 0));
            lgs.Add(new Tuple<int, double>(sortedItemList.Count - 1, 0));

            var result = new List<List<Point>>();
            for (int i = 0; i < lgs.Count - 1; i++)
            {
                Console.WriteLine("Taking {0} to {1}", lgs[i], lgs[i + 1]);
                List<Point> tmp = new List<Point>();

                int start = lgs[i].Item1;
                int end = lgs[i + 1].Item1;
                var tmp2 = sortedItemList.Skip(lgs[i].Item1).Take(end - start + 1).ToList();
                result.Add(tmp2);
            }
            return result;
        }

        public List<Tuple<int, double>> LargestGaps(List<Point> sortedItemList)
        {
            int index = 0;
            double largestGap = 0.0;
            List<Tuple<int, double>> gaps = new List<Tuple<int, double>>();
            for (int i = 1; i < sortedItemList.Count - 1; i++)
            {
                var gap = Math.Abs(sortedItemList[i].Eigen[0] - sortedItemList[i - 1].Eigen[0]);
                gaps.Add(new Tuple<int, double>(i, gap));
                if (gap > largestGap)
                {
                    index = i;
                    largestGap = gap;
                }
            }
            double epsilon = 2.50948562060746E-7;
            gaps = gaps.OrderByDescending(x => x.Item2).Where(x => x.Item2 > epsilon).ToList();
            return gaps;
        }

    }
}
