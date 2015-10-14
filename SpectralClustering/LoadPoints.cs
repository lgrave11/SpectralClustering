using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace SpectralClustering
{
    public class LoadPoints
    {
        public List<Point> points;
        public LoadPoints(string filename)
        {
            this.points = new List<Point>();
            foreach (string line in File.ReadLines(filename))
            {
                var proc = line.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');
                Point p = new Point(Convert.ToInt32(proc[0]), Convert.ToInt32(proc[1]));
                points.Add(p);
            }
        }
    }
}
