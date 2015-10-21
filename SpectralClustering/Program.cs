using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Drawing;
using System.IO;
using MathNet.Numerics.Data.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using ColorMine.ColorSpaces;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace SpectralClustering
{
    class Program
    {
        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(int H, int L, int S);
        public static double RBFKernel(Point p1, Point p2)
        {
            return Math.Exp(-(Math.Pow(p1.dist(p2), 2)) / (2*Math.Pow(0.5, 2)));
        }
        public static double RBFKernel2(Point p1, Point p2)
        {
            return Math.Exp(-(Math.Pow(p1.manhattan_dist(p2), 2)) / (2 * Math.Pow(0.5, 2)));
        }
        public static double EuclideanDistance(Point p1, Point p2)
        {
            return -p1.dist(p2);
        }
        public static double CosineSimilarity(Point p1, Point p2)
        {
            double[] p1a = { p1.x, p1.y };
            double[] p2a = { p2.x, p2.y };
            return Math.Exp(-(Math.Pow(Distance.Euclidean(p1a, p2a), 2) / (2 * Math.Pow(0.5, 2))));
        }

        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            List<string> filenames = new List<string> { "coords12.png", "coords2.png", "coords3.png", "coords4.png", "coords5.png", "coords6.png", "coords8.png", "coords9.png", "cluster10.png", "coords11.png" };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var f in filenames)
            {
                List<Point> lp = GetPoints(f);
                //KMeans kmeans = new KMeans(lp, 3);
                //DrawCommunities(kmeans.clusters, f, "kmeans");
                DBSCAN dbscan = new DBSCAN(lp, 3, 3);
                dbscan.Run();
                DrawCommunities(dbscan.clusters, f, "dbscan");
                //Console.WriteLine("Producing clusters for {0}", f);
                //List<List<Point>> communities = FindCommunities(lp,RBFKernel);
                //DrawCommunities(communities, f, "RBFKernelEuclidean");
                ///*List<List<Point>> communities2 = FindCommunities(lp, RBFKernel2);
                //DrawCommunities(communities2, f, "RBFKernelManhattan");*/
            }
            sw.Stop();
            TimeSpan elapsedTime = sw.Elapsed;
            Console.WriteLine(elapsedTime);

            sw = new Stopwatch();
            sw.Start();
            foreach (var f in filenames)
            {
                List<Point> lp = GetPoints(f);
                //KMeans kmeans = new KMeans(lp, 3);
                //DrawCommunities(kmeans.clusters, f, "kmeans");
                DBSCANOld dbscan = new DBSCANOld(lp, 3, 3);
                dbscan.Run();
                DrawCommunities(dbscan.clusters, f, "dbscan");
                //Console.WriteLine("Producing clusters for {0}", f);
                //List<List<Point>> communities = FindCommunities(lp,RBFKernel);
                //DrawCommunities(communities, f, "RBFKernelEuclidean");
                ///*List<List<Point>> communities2 = FindCommunities(lp, RBFKernel2);
                //DrawCommunities(communities2, f, "RBFKernelManhattan");*/
            }
            sw.Stop();
            elapsedTime = sw.Elapsed;
            Console.WriteLine(elapsedTime);

        }

        public static void DrawCommunities(List<List<Point>> lp, string filename, string prefix)
        {
            Bitmap img = new Bitmap(filename);
            Bitmap bm = new Bitmap(img);

            int index = 0;
            Random r = new Random();
            var colors = Enumerable.Range(0, 360).Where((x, i) => i % 30 == 0).ToList();
            var colors2 = colors.Select(x => new Hsl { H = x, S = 100, L = 50 }).ToList();
            colors2.Insert(0, new Hsl { H = 0, S = 100, L = 0 });
            foreach (var community in lp)
            {
                var value = new Hsl { H = r.Next(0, 360), S = r.Next(50, 100), L = r.Next(40, 60) };
                //var value = colors2[index];
                var rgb = value.To<Rgb>();
                Color c = Color.FromArgb(255, (int)rgb.R, (int)rgb.G, (int)rgb.B);
                foreach (var v in community)
                {
                    bm.SetPixel((int)v.x, (int)v.y, c);
                }
                index += 1;
            }
            bm.Save(Path.Combine("output2", Path.GetFileNameWithoutExtension(filename) + "_" + prefix + "_bm.png"));
        } 

        public static List<Point> GetPoints(string filename)
        {
            var r = new Random();
            
            List<Point> lp = new List<Point>();
            Bitmap img = new Bitmap(filename);
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                    {
                        lp.Add(new Point(i, j));
                    }
                }
            }
            lp = lp.OrderBy(x => r.Next()).ToList();

            return lp;
        }

        public static List<List<Point>> FindCommunities(List<Point> lp, Func<Point, Point, double> similarityMeasure)
        {
            List<List<Point>> allCommunities = new List<List<Point>>();
            List<Point> eigenDecomposed = SpectralClustering(lp, similarityMeasure);
            List<List<Point>> cutCommunities = new List<List<Point>>();
            cutCommunities = Cut(eigenDecomposed);
            allCommunities.AddRange(cutCommunities);
            //foreach (var v in cutCommunities)
            //{
            //    List<List<Point>> subCommunities = new List<List<Point>>();
            //    var eig = SpectralClustering(v, similarityMeasure);
            //    allCommunities.AddRange(Cut(eig));
            //
            //}
            return allCommunities;
        }

        public static List<Point> SpectralClustering(List<Point> lp, Func<Point, Point, double> similarityMeasure)
        {
            Console.WriteLine("Making matrixes");
            if (lp.Count == 1) return lp;
            List<List<Double>> S = new List<List<double>>();
            for (int i = 0; i < lp.Count; i++)
            {
                List<double> row = new List<double>();
                for (int j = 0; j < lp.Count; j++)
                {
                    if (i == j)
                    {
                        row.Add(0f);
                    }
                    else
                    {
                        row.Add(similarityMeasure(lp[i], lp[j]));
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
            Console.WriteLine("Finding EVD.");
            Evd<double> evd = L.Evd();
            Vector<double> eigenVector = evd.EigenVectors.Column(1);
            Chart c = new Chart();
            c.ChartAreas.Add("EigenVectors");

            c.Series.Add("bla");
            c.Series["bla"].ChartType = SeriesChartType.Point;
            c.Series["bla"].Color = Color.LightGreen;
            double xIndex = 0;
            foreach(var p in evd.EigenVectors.Column(2).OrderBy(x => x).ToList())
            {
                c.Series["bla"].Points.AddXY(xIndex++, p);
            }
            
            c.SaveImage("Blah.png", ChartImageFormat.Png);
            Console.WriteLine("Adding eigen vector value to users.");
            for (int ev = 0; ev < eigenVector.Count; ev++)
            {
                //lp[ev].Eigen.Add(eigenVector[ev]);
                //lp[ev].Eigen = eigenVector[ev];
                lp[ev].Eigen.AddRange(evd.EigenVectors.Row(ev).Skip(1).Take(10));
            }
            List<double> eigengaps = new List<double>();
            //var eigenVector2 = eigenVector.OrderBy(x => x).ToList();
            var eigenValues = evd.EigenValues.Select(x => x.Real).ToList();
            var tmp = EigengapHeuristic(eigenVector.OrderBy(x => x).ToList());
            var sortedItemList = lp.OrderBy(x => x.Eigen[0]).ToList();
            var tmp2 = LargestGap(sortedItemList, 0);
            return sortedItemList;
        }

        public static List<Tuple<int, double>> EigengapHeuristic(List<double> eigenvalues)
        {
            List<Tuple<int, double>> gaps = new List<Tuple<int, double>>();
            //int index = 0;
            //double largestGap = 0.0;
            for (int i = 1; i < eigenvalues.Count; i++)
            {
                var gap = Math.Abs(eigenvalues[i] - eigenvalues[i - 1]);
                gaps.Add(new Tuple<int, double>(i, gap));
                /*if (gap > largestGap)
                {
                    index = i;
                    largestGap = gap;
                }*/
            }
            return gaps.OrderByDescending(x => x.Item2).ToList();
        }

        public static List<List<Point>> Cut(List<Point> sortedItemList)
        {
            int eigenIndex = 0;
            var lg = LargestGap(sortedItemList, eigenIndex);
            var lgs = LargestGaps(sortedItemList, eigenIndex).OrderBy(x => x).ToList();
            lgs.Insert(0, 0);
            lgs.Add(sortedItemList.Count - 1);

            var result = new List<List<Point>>();
            for(int i = 0; i < lgs.Count-1; i++)
            {
                Console.WriteLine("Taking {0} to {1}", lgs[i], lgs[i + 1]);
                List<Point> tmp = new List<Point>();
                
                int start = lgs[i];
                int end = lgs[i + 1];
                var tmp2 = sortedItemList.Skip(lgs[i]).Take(end - start+1).ToList();
                /*int current = lgs[i];
                foreach (var v in sortedItemList.Skip(lgs[i]))
                {
                    tmp.Add(sortedItemList[current++]);
                    if (current == end) break;
                }*/
                //var tmp = sortedItemList.Skip(lgs[i]).Take(lgs[i + 1]).ToList();
                result.Add(tmp2);
            }
            /*Console.WriteLine("Making new lists.");
            List<Point> ListLeft = sortedItemList.Take(lg.Item1 + 1).ToList();
            List<Point> ListRight = sortedItemList.Skip(lg.Item1 + 1).ToList();
            var res= new List<List<Point>> { ListLeft, ListRight };
            return res;*/
            return result;
        }

        public static List<int> LargestGaps(List<Point> sortedItemList, int eigenIndex)
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
            return gaps.Select(x => x.Item1).ToList();
            //return new Tuple<int, double>(index, largestGap);
        }

        public static Tuple<int, double> LargestGap(List<Point> sortedItemList, int eigenIndex)
        {
            int index = 0;
            double largestGap = 0.0;
            //var tmp = sortedItemList.OrderBy(x => x.Eigen[eigenIndex]).ToList();
            for (int i = 0; i < sortedItemList.Count - 1; i++)
            {
                var gap = Math.Abs(sortedItemList[i].Eigen[eigenIndex] - sortedItemList[i + 1].Eigen[eigenIndex]);
                if (gap > largestGap)
                {
                    index = i;
                    largestGap = gap;
                }
            }
            return new Tuple<int, double>(index, largestGap);
        }

        public static void WriteImage(Matrix<double> matrix, bool special = false, string filename = "matrix")
        {
            Bitmap bm = new Bitmap(matrix.ColumnCount, matrix.ColumnCount);
            for (int m = 0; m < matrix.RowCount; m++)
            {
                for (int n = 0; n < matrix.ColumnCount; n++)
                {
                    if (!special)
                    {
                        if (matrix[m, n] == 0f)
                            bm.SetPixel(m, n, Color.White);
                        else
                            bm.SetPixel(m, n, Color.FromArgb(Math.Abs((int)matrix[m, n]) % 255, Math.Abs((int)matrix[m, n]) % 255, Math.Abs((int)matrix[m, n]) % 255));
                    }
                    else
                    {
                        if (matrix[m, n] == 0f)
                            bm.SetPixel(m, n, Color.White);
                        else if (matrix[m, n] == 1f)
                            bm.SetPixel(m, n, Color.Blue);
                        else if (matrix[m, n] == 2f)
                            bm.SetPixel(m, n, Color.Red);
                    }

                }
            }
            bm.Save(Path.Combine("output", filename + ".png"));
        }
    }
}
