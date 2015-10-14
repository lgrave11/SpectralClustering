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

namespace SpectralClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            FindCommunities();
        }

        public static void FindCommunities()
        {
            Console.WriteLine("Making matrixes");
            List<Point> lp = new List<Point>();
            Bitmap img = new Bitmap("coords6.png");
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
            var r = new Random();
            lp = lp.OrderBy(x => r.Next()).ToList();
            //lp = lp.OrderBy(x => x.y).ToList();
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
                        var d = Math.Pow(lp[i].dist(lp[j]), 2);
                        if (d > 25)
                            row.Add(0);
                        else
                            row.Add(Math.Exp(-0.5 * d));
                    }
                }
                S.Add(row);
            }

            /*for(int x=0; x < S.Count; x++)
            {
                for (int y = 0; y < S[x].Count; y++)
                {
                    lp.points.
                }
            }*/

            /*Matrix<double> Abla = DelimitedReader.Read<double>("cluster6.txt", false, ",", false);
            List<Point> lp2 = new List<Point>();
            for (int x = 0; x < Abla.RowCount; x++)
            {
                lp2.Add(new Point(x, 0));
            }*/

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
            /*for (int x = 0; x < S.Count; x++)
            {
                for (int y = 0; y < S[x].Count; y++)
                {
                    A[x, y] = S[x][y];
                }
            }*/
            /*
               CultureInfo ci = new CultureInfo("en-US");
                using (StreamWriter sw = new StreamWriter("cluster7.txt"))
                {
                    for (int m = 0; m < A.RowCount; m++)
                    {
                        for (int n = 0; n < A.ColumnCount; n++)
                        {

                            sw.Write(String.Format(ci, "{0:0.00},", A[m, n]));
                        }
                        sw.Write("\n");
                    }
                }*/
            //Matrix<double> A = Matrix<double>.Build.Dense(notAdjacency.ColumnCount, notAdjacency.RowCount);
            //foreach(var item in itemList)
            //{
            //    foreach(var item2 in itemList)
            //    {
            //        int res = 0;
            //        if((item.x == item2.x && item.y == item2.y) || (item.value == 0 || item2.value == 0))
            //        {
            //            res = 0;
            //        }
            //        else
            //        {
            //            if(item.value == 1 && item2.value == 1)
            //            {
            //                res = 1;
            //            }
            //            //res = ;//Math.Sqrt(Math.Pow(item.x - item2.x, 2) + Math.Pow(item.y - item2.y, 2));
            //            A[item.x, item.y] = res;
            //            item.value = res;
            //            item2.value = res;
            //        }
            //        
            //    }
            //}


            Vector<double> dVector = A.RowAbsoluteSums();
            Matrix<double> D = Matrix<double>.Build.DenseOfDiagonalVector(dVector);
            Matrix<double> L = D - A;
            Console.WriteLine("Finding EVD.");
            Evd<double> evd = L.Evd();
            Vector<double> eigenVector = evd.EigenVectors.Column(1);
            
            Console.WriteLine("Adding eigen vector value to users.");
            for (int ev = 0; ev < eigenVector.Count; ev++)
            {
                lp[ev].Eigen = eigenVector[ev];
            }
            var sortedItemList = lp.OrderBy(x => x.Eigen).ToList();
            Console.WriteLine("Cutting communities.");
            Cut(sortedItemList);
            //sortedItemList = sortedItemList.OrderBy(x => x.x).ToList();
            Matrix<double> A2 = Matrix<double>.Build.Dense(A.ColumnCount, A.RowCount);
            
            for (int m = 0; m < A2.RowCount; m++)
            {
                for (int n = 0; n < A2.ColumnCount; n++)
                {
                    //Console.WriteLine("{0},{1}: {2} {3}", m,n, sortedItemList[m].clusternum, sortedItemList[n].clusternum);
                    A2[m,n] = (A[m,n] > 0) ? sortedItemList[m].clusternum : 0;
                    A2[n, m] = (A[n, m] > 0) ? sortedItemList[m].clusternum : 0;
                }
            }
            WriteImage(A2, true, "matrix");
            WriteImage(A, false, "original");

            Bitmap bm = new Bitmap(img);
            /*for(int i = 0; i < bm.Width; i++)
            {
                for (int j = 0; j < bm.Height; j++)
                {
                    bm.SetPixel(i, j, Color.Gray);
                }
            }*/
            foreach(var v in lp)
            {
                bm.SetPixel(v.x, v.y, v.clusternum == 1 ? Color.Blue : v.clusternum == 2 ? Color.Red : Color.Black);
            }
            bm.Save("bm.png");

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
                            bm.SetPixel(m, n, Color.FromArgb((int)matrix[m, n] % 255, (int)matrix[m, n] % 255, (int)matrix[m, n] % 255));
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
            bm.Save(filename + ".png");
        }

        public static void Cut(List<Point> sortedItemList)
        {
            double largestGap = 0.0;
            int index = 0;
            for (int i = 0; i < sortedItemList.Count - 1; i++)
            {
                var gap = Math.Abs(sortedItemList[i].Eigen - sortedItemList[i + 1].Eigen);
                if (gap > largestGap)
                {
                    index = i;
                    largestGap = gap;
                }
            }
        
            Console.WriteLine("Making new lists.");
            for(int i=0; i<sortedItemList.Count; i++)
            {
                if(i <= index+1)
                {
                    sortedItemList[i].clusternum = 1f;
                }
                else
                {
                    sortedItemList[i].clusternum = 2f;
                }
            }
            //List<Point> ListLeft = sortedItemList.Take(index + 1).ToList();
            //List<Point> ListRight = sortedItemList.Skip(index + 1).ToList();
            //return new Tuple<List<Point>, List<Point>>(ListLeft, ListRight);
        }
    }
}
