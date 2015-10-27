using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace SpectralClustering
{
    public class SpectralClustering
    {
        List<Point> points;
        public List<List<Point>> clusters;
        int maxClusters;
        Func<Point, Point, Double> similarityMeasure;
        public SpectralClustering(List<Point> lp, Func<Point, Point, double> similarityMeasure, int maxClusters = 10)
        {
            this.points = lp;
            this.similarityMeasure = similarityMeasure;
            this.maxClusters = maxClusters;
            this.clusters = new List<List<Point>>();
        }

        public void Run()
        {
            List<Point> eigenDecomposed = EigenDecomposition();
            List<List<Point>> cutCommunities = new List<List<Point>>();
            cutCommunities = Cut(eigenDecomposed, maxClusters);
            clusters.AddRange(cutCommunities);
        }

        public List<Point> EigenDecomposition()
        {
            Console.WriteLine("Making matrixes");
            if (this.points.Count == 1) return this.points;
            List<List<Double>> S = new List<List<double>>();
            for (int i = 0; i < this.points.Count; i++)
            {
                List<double> row = new List<double>();
                for (int j = 0; j < this.points.Count; j++)
                {
                    if (i == j)
                    {
                        row.Add(0f);
                    }
                    else
                    {
                        row.Add(similarityMeasure(this.points[i], this.points[j]));
                    }
                }
                S.Add(row);
            }

            var asm = S.Select(Enumerable.ToArray).ToArray();
            double[,] asm2 = new double[asm.Length, asm.Max(x => x.Length)];
            for (var i = 0; i < asm.Length; i++)
            {
                for (var j = 0; j < asm[i].Length; j++)
                {
                    asm2[i, j] = asm[i][j];
                }
            }
            Matrix<double> A = Matrix<double>.Build.DenseOfArray(asm2);
            Vector<double> dVector = A.RowAbsoluteSums();
            Matrix<double> D = Matrix<double>.Build.DenseOfDiagonalVector(dVector);
            Matrix<double> L = D - A;
            //Matrix<double> L = Matrix<double>.Build.DenseIdentity(D.RowCount, D.ColumnCount) - D.Inverse() * A;
            /*Matrix<double> I = Matrix<double>.Build.DenseIdentity(D.RowCount, D.ColumnCount);
            Vector<double> dVector2 = Vector<double>.Build.DenseOfEnumerable(dVector.Select(x => Math.Pow(x, -0.5)));
            Matrix<double> D2 = Matrix<double>.Build.DenseOfDiagonalVector(dVector);
            Matrix<double> L = I - D2 * A * D2;
            dVector2.Select(x => Math.Pow(x, -0.5));*/
            Console.WriteLine("Finding EVD.");
            Evd<double> evd = L.Evd();
            Vector<double> eigenVector = evd.EigenVectors.Column(1);
            Console.WriteLine("Adding eigen vector value to users.");
            for (int ev = 0; ev < eigenVector.Count; ev++)
            {
                this.points[ev].Eigen.AddRange(evd.EigenVectors.Row(ev).Skip(1).Take(10));
            }
            List<double> eigengaps = new List<double>();
            var sortedItemList = this.points.OrderBy(x => x.Eigen[0]).ToList();
            return sortedItemList;
        }

        public List<List<Point>> Cut(List<Point> sortedItemList, int maxClusters)
        {
            int eigenIndex = 0;
            var lgs = LargestGaps(sortedItemList, eigenIndex).ToList();
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

        public List<Tuple<int, double>> LargestGaps(List<Point> sortedItemList, int eigenIndex)
        {
            int index = 0;
            double largestGap = 0.0;
            List<Tuple<int, double>> gaps = new List<Tuple<int, double>>();
            for (int i = 1; i < sortedItemList.Count - 1; i++)
            {
                var gap = Math.Abs(sortedItemList[i].Eigen[eigenIndex] - sortedItemList[i - 1].Eigen[eigenIndex]);
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
