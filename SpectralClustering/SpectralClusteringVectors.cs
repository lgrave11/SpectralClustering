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
    public class SpectralClusteringVectors
    {
        List<Vector<double>> vectors;
        public List<List<Vector<double>>> clusters;
        int maxClusters;
        Func<Vector<double>, Vector<double>, Double> similarityMeasure;
        Evd<double> evd;
        public SpectralClusteringVectors(List<Vector<double>> lp,
                                         Func<Vector<double>, Vector<double>, Double> similarityMeasure,
                                         int maxClusters = 10)
        {
            this.vectors = lp;
            this.similarityMeasure = similarityMeasure;
            this.maxClusters = maxClusters;
            this.clusters = new List<List<Vector<double>>>();
        }

        public void Run()
        {
            if(vectors.Count == 1)
            {
                clusters.Add(new List<Vector<double>> { vectors.First() });
            }
            var vec2eigen = PerformEigenDecomposition();
            {
                List<List<Vector<double>>> cutClusters;
                cutClusters = Cut(vec2eigen);
                clusters.AddRange(cutClusters);
            }
            
        }

        public Dictionary<Vector<double>, double> PerformEigenDecomposition()
        {
            //Console.WriteLine("Making matrixes");
            
            double[,] S = new double[this.vectors.Count, this.vectors.Count];
            //List<List<Double>> S = new List<List<double>>();
            for (int i = 0; i < this.vectors.Count; i++)
            {
                for (int j = 0; j < this.vectors.Count; j++)
                {
                    if (i == j)
                    {
                        S[i, j] = 0f;
                    }
                    else
                    {
                        S[i, j] = similarityMeasure(this.vectors[i], this.vectors[j]);
                    }
                }
            }
            Matrix<double> A = Matrix<double>.Build.DenseOfArray(S);
            using (TextWriter tw = new StreamWriter("Avec.txt"))
            {
                for (int j = 0; j < this.vectors.Count; j++)
                {
                    for (int i = 0; i < this.vectors.Count; i++)
                    {
                        tw.Write("{0:0.00} ", A[i, j]);
                    }
                    tw.WriteLine();
                }
            }
            Vector<double> dVector = A.RowAbsoluteSums();
            Matrix<double> D = Matrix<double>.Build.DenseOfDiagonalVector(dVector);
            Matrix<double> I = Matrix<double>.Build.DenseIdentity(D.RowCount, D.ColumnCount);
            Matrix<double> L;
            L = D - A;
            //L = D.Inverse() * A;
            //Vector<double> dVector2 = Vector<double>.Build.DenseOfEnumerable(dVector.Select(x => Math.Pow(x, -0.5)));
            //Matrix<double> D2 = Matrix<double>.Build.DenseOfDiagonalVector(dVector2);
            //L = I - D2 * A * D2;

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
            foreach (var p in eigenVectors2.OrderByDescending(x => x))
            {
                c.Series["bla"].Points.AddXY(xIndex++, p);
            }
            var fn = String.Format("{0}.png", this.vectors.Count);
            c.SaveImage(fn, ChartImageFormat.Png);

            this.evd = evd;
            int ev = 0;
            Dictionary<Vector<double>, double> vec2eigen = new Dictionary<Vector<double>, double>();
            foreach (var v in this.vectors)
            {
                vec2eigen[v] = eigenVector[ev];
                ev++;
            }
            return vec2eigen;
        }

        public List<List<Vector<double>>> Cut(Dictionary<Vector<double>, double> vec2eigen)
        {
            var lgs = LargestGaps(vec2eigen).ToList();
            var sortedVectorList = vec2eigen.OrderBy(x => x.Value).ToList();
            lgs = lgs.Take(maxClusters - 1).OrderBy(x => x.Item1).ToList();
            lgs.Insert(0, new Tuple<int, double>(0, 0));
            lgs.Add(new Tuple<int, double>(sortedVectorList.Count - 1, 0));

            var result = new List<List<Vector<double>>>();
            for (int i = 0; i < lgs.Count - 1; i++)
            {
                Console.WriteLine("Taking {0} to {1}", lgs[i], lgs[i + 1]);
                var tmp = new List<Vector<double>>();

                int start = lgs[i].Item1;
                int end = lgs[i + 1].Item1;
                var tmp2 = sortedVectorList.Skip(lgs[i].Item1).Take(end - start + 1).Select(x => x.Key).ToList();
                result.Add(tmp2);
            }
            return result;
        }

        public List<Tuple<int, double>> LargestGaps(Dictionary<Vector<double>, double> vec2eigen)
        {
            var sortedVectorList = vec2eigen.OrderBy(x => x.Value).ToList();
            int index = 0;
            double largestGap = 0.0;
            List<Tuple<int, double>> gaps = new List<Tuple<int, double>>();
            for (int i = 1; i < sortedVectorList.Count - 1; i++)
            {
                var gap = Math.Abs(sortedVectorList[i].Value - sortedVectorList[i - 1].Value);
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
