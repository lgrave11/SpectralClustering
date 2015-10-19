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
            List<string> filenames = new List<string> { "coords8.png" };
            foreach(var f in filenames)
            {
                List<Point> lp = GetPoints(f);
                Console.WriteLine("Producing clusters for {0}", f);
                List<List<Point>> communities = FindCommunities(lp,RBFKernel, 3);
                DrawCommunities(communities, f, "RBFKernelEuclidean");
                /*List<List<Point>> communities2 = FindCommunities(lp, RBFKernel2);
                DrawCommunities(communities2, f, "RBFKernelManhattan");*/
            }
            
        }

        public static void DrawCommunities(List<List<Point>> lp, string filename, string prefix)
        {
            Bitmap img = new Bitmap(filename);
            Bitmap bm = new Bitmap(img);

            int index = 0;
            Random r = new Random(1);
            //var colors = Enumerable.Range(0, 360).Where((x, i) => i % 5 == 0).ToList();
            foreach (var community in lp)
            {
                var value = r.Next(0, 360);
                var hsl = new Hsl { H = value, S = 100, L = 50 };
                var rgb = hsl.To<Rgb>();
                Color c = Color.FromArgb(255, (int)rgb.R, (int)rgb.G, (int)rgb.B);
                foreach (var v in community)
                {
                    bm.SetPixel(v.x, v.y, c);
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

        public static List<List<Point>> FindCommunities(List<Point> lp, Func<Point, Point, double> similarityMeasure, int k)
        {
            Console.WriteLine("Making matrixes");

            if (lp.Count == 1) return new List<List<Point>>() { lp };
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
            //Matrix<double> AMark = D.Inverse() * A;
            Matrix<double> L = D - A;
            Console.WriteLine("Finding EVD.");
            Evd<double> evd = L.Evd();
            Vector<double> eigenVector = evd.EigenVectors.Column(1);
            Chart c = new Chart();
            c.ChartAreas.Add("EigenVectors");

            c.Series.Add("bla");
            c.Series["bla"].ChartType = SeriesChartType.Point;
            c.Series["bla"].Color = Color.LightGreen;
            double xIndex = 0;
            foreach(var p in evd.EigenVectors.Column(0).OrderBy(x => x))
            {
                c.Series["bla"].Points.AddXY(xIndex++, p);
            }
            
            c.SaveImage("Blah.png", ChartImageFormat.Png);

            Console.WriteLine("Adding eigen vector value to users.");
            for (int ev = 0; ev < eigenVector.Count; ev++)
            {
                //lp[ev].Eigen.Add(eigenVector[ev]);
                lp[ev].Eigen.AddRange(evd.EigenVectors.Row(ev).Skip(1).Take(5));
            }
            var sortedItemList = lp.OrderBy(x => x.Eigen[0]).ToList();
            Console.WriteLine("Cutting communities.");
            List<List<Point>> cutCommunities = Cut(sortedItemList, 3);

            if (cutCommunities == null)
            {
                return new List<List<Point>>() { sortedItemList };
            }
            //var coms = FindCommunities(cutCommunities.Item1, similarityMeasure, k);
            //var coms2 = FindCommunities(cutCommunities.Item2, similarityMeasure, k);
            //if(coms != null)
            //{
            //    foreach (var x in coms)
            //    {
            //        allCommunities.Add(x);
            //    }
            //}
            //if(coms2 != null)
            //{
            //    foreach (var x in coms2)
            //    {
            //        allCommunities.Add(x);
            //    }
            //}
            
            
            return cutCommunities;
            //Matrix<double> A2 = Matrix<double>.Build.Dense(A.ColumnCount, A.RowCount);
            //
            //for (int m = 0; m < A2.RowCount; m++)
            //{
            //    for (int n = 0; n < A2.ColumnCount; n++)
            //    {
            //        //Console.WriteLine("{0},{1}: {2} {3}", m,n, sortedItemList[m].clusternum, sortedItemList[n].clusternum);
            //        A2[m,n] = (A[m,n] > 0) ? sortedItemList[m].clusternum : 0;
            //        A2[n, m] = (A[n, m] > 0) ? sortedItemList[m].clusternum : 0;
            //    }
            //}
            //WriteImage(A2, true, Path.GetFileNameWithoutExtension(filename) + "_" + prefix +"_matrix");
            //WriteImage(A, false, Path.GetFileNameWithoutExtension(filename) + "_" + prefix + "_original");
            //
            //Bitmap bm = new Bitmap(img);
            //foreach(var v in lp)
            //{
            //    bm.SetPixel(v.x, v.y, v.clusternum == 1 ? Color.Blue : v.clusternum == 2 ? Color.Red : Color.Black);
            //}
            //bm.Save(Path.Combine("output2", Path.GetFileNameWithoutExtension(filename) + "_" + prefix + "_bm.png"));

            //if (cutCommunities == null)
            //{
            //    return new List<List<User>>() { userList };
            //}
            //
            //List<List<User>> allCommunities = new List<List<User>>();
            //foreach (var x in FindCommunities(cutCommunities.Item1))
            //{
            //    allCommunities.Add(x);
            //}
            //foreach (var x in FindCommunities(cutCommunities.Item2))
            //{
            //    allCommunities.Add(x);
            //}
            //return allCommunities;
        }

        public static void WriteImage(Matrix<double> matrix, bool special = false, string filename = "matrix")
        {
            Bitmap bm = new Bitmap(matrix.ColumnCount, matrix.ColumnCount);
            for (int m = 0; m < matrix.RowCount; m++)
            {
                for (int n = 0; n < matrix.ColumnCount; n++)
                {
                    if(!special)
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

        public static List<List<Point>> Cut(List<Point> sortedItemList, int amount)
        {
            List<Tuple<int, double>> gaps = new List<Tuple<int, double>>();
            //gaps.Add(LargestGap(sortedItemList, 0));
            for(int i = 0; i < amount-1; i++)
            {
                gaps.Add(LargestGap(sortedItemList, i));
            }
            List<List<Point>> communities = new List<List<Point>>();
            gaps = gaps.OrderBy(x => x.Item1).ToList();
            var tmp = sortedItemList.ToList();
            foreach (var i in gaps)
            {
                if(i.Item2 > 0.7)
                {
                    break;
                }
                List<Point> ListLeft = sortedItemList.Take(i.Item1 + 1).ToList();
                List<Point> ListRight = sortedItemList.Skip(i.Item1 + 1).ToList();
                communities.Add(ListLeft);
                communities.Add(ListRight);
            }
            return communities;
            /*if (largestGap > 0.2)
            {
                return null;
            }


            Console.WriteLine("Making new lists.");
            List<Point> ListLeft = sortedItemList.Take(index + 1).ToList();
            List<Point> ListRight = sortedItemList.Skip(index + 1).ToList();
            return new List<List<Point>> { ListLeft, ListRight };*/
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
    }
}
