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

        static void Main(string[] args)
        {
            Control.UseNativeMKL();
            List<string> filenames = new List<string> {"Examples/tiny.png",
                                                       //"Examples/a.png",
                                                       "Examples/AB.png",
                                                       "Examples/aimdisk.png",
                                                       "Examples/four_dots.png",
                                                       "Examples/nine_dots.png",
                                                       "Examples/noise.png",
                                                       "Examples/oldfaithful.png",
                                                       //"Examples/target.png",
                                                       //"Examples/target2.png",
                                                       "Examples/two_bananas.png",
                                                       "Examples/two_dots.png",
                                                       "Examples/shapes.png",
                                                      };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var f in filenames)
            {
                Console.WriteLine("On {0} for Spectral", f);
                List<Point> lp1 = GetPoints(f);
                SpectralClustering sc = new SpectralClustering(lp1, DistanceFunctions.RBFKernel);
                sc.Run();
                DrawCommunities(sc.clusters, f, "RBF");
                //var lp2 = GetPoints(f);
                //SpectralClustering sc2 = new SpectralClustering(lp2, DistanceFunctions.RBFKernel, maxClusters: 9);
                //sc2.Run();
                //DrawCommunities(sc2.clusters, f, "RBF");
                //var lp2 = GetPoints(f);
                //SpectralClustering sc2 = new SpectralClustering(lp2, DistanceFunctions.SquaredEuclideanDistance, maxClusters: 2);
                //sc2.Run();
                //DrawCommunities(sc2.clusters, f, "SED");
            }
            sw.Stop();
            TimeSpan elapsedTime = sw.Elapsed;
            Console.WriteLine("Spectral: {0}", elapsedTime);

            /*sw = new Stopwatch();
            sw.Start();
            foreach (var f in filenames)
            {
                Console.WriteLine("On {0} for DBSCAN", f);
                List<Point> lp = GetPoints(f);
                //KMeans kmeans = new KMeans(lp, 2);
                //DrawCommunities(kmeans.clusters, f, "kmeans");
                DBSCAN dbscan = new DBSCAN(lp, Eps:3, MinPts:3);
                dbscan.Run();
                DrawCommunities(dbscan.clusters, f, "dbscan");
            }
            sw.Stop();
            elapsedTime = sw.Elapsed;
            Console.WriteLine("DBSCAN: {0}", elapsedTime);

            sw = new Stopwatch();
            sw.Start();
            foreach (var f in filenames)
            {
                Console.WriteLine("On {0} for KMeans", f);
                List<Point> lp = GetPoints(f);
                KMeans kmeans = new KMeans(lp, 6);
                DrawCommunities(kmeans.clusters, f, "kmeans");
            }
            sw.Stop();
            elapsedTime = sw.Elapsed;
            Console.WriteLine("KMeans: {0}", elapsedTime);*/
        }

        public static void DrawCommunities(List<List<Point>> lp, string filename, string prefix)
        {
            Bitmap img = new Bitmap(filename);
            Bitmap bm = new Bitmap(img);
            lp = lp.OrderBy(x => x.OrderBy(y => y.x).First().x).ToList();
            int index = 0;
            Random r = new Random(1);
            var colors = Enumerable.Range(0, 360).Where((x, i) => i % 30 == 0).ToList();
            var colors2 = colors.Select(x => new Hsl { H = x, S = 100, L = 50 }).ToList();
            var colors3 = new List<Hsl> { new Hsl { H = 0, S = 100, L = 50 }, new Hsl { H = 233, S = 100, L = 50 } };
            //colors2.Insert(0, new Hsl { H = 0, S = 100, L = 0 });
            foreach (var community in lp)
            {
                //var value = new Hsl { H = r.Next(0, 360), S = r.Next(50, 100), L = r.Next(40, 60) };
                //var value = colors2[index];
                Hsl value = null;
                try {
                    value = colors3[lp.IndexOf(community)];
                }
                catch (Exception e)
                {
                    value = new Hsl { H = r.Next(0, 360), S = r.Next(50, 100), L = r.Next(40, 60) };
                }

                //var value = colors2[lp.IndexOf(community)];

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
