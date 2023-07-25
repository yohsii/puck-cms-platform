using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Net;

namespace puck.core.ImageRecognition
{
    public class Normal
    {
        public double NormalR { get; set; } = 0;
        public double NormalG { get; set; } = 0;
        public double NormalB { get; set; } = 0;
    }
    public static class Similarity
    {
        public static double Sample(Uri fileURL) {
            Stream stream = null;
            using (WebClient client = new WebClient())
            {
                stream = client.OpenRead(fileURL);
            }
            return Sample(stream);
        }
        public static double Sample(string filepath) {
            var stream = File.OpenRead(filepath);
            return Sample(stream);
        }
        public static double Sample(Stream stream)
        {

            var normal = new Normal();

            Bitmap img = new Bitmap(stream);
            double totalR = 0;
            double totalG = 0;
            double totalB = 0;
            double totalBright = (double)0;
            var posstotal = 0;

            for (int i = 0; i < img.Width; i++)
            {
                if (i == 0)
                {
                    posstotal = (img.Width * img.Height) * 255;
                }
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    var r = pixel.R;
                    var g = pixel.G;
                    var b = pixel.B;
                    var bright = System.Math.Sqrt((r * r * .241) + (g * g * .691) + (b * b * .068));
                    totalR += r;
                    totalG += g;
                    totalB += b;
                    totalBright += bright;
                }

            }

            var finalR = Math.Round((totalR / posstotal) * totalBright);
            var finalG = Math.Round((totalG / posstotal) * totalBright);
            var finalB = Math.Round((totalB / posstotal) * totalBright);

            var inverseR = Math.Round(((1 * 10) / finalR) * 10, 10);
            var inverseG = Math.Round(((1 * 10) / finalG) * 10, 10);
            var inverseB = Math.Round(((1 * 10) / finalB) * 10, 10);

            var oppositeR = Math.Round(finalR * (1 / -10));
            var oppositeG = Math.Round(finalG * (1 / -10));
            var oppositeB = Math.Round(finalB * (1 / -10));

            var normalR = Math.Round(Math.Sqrt((finalR / inverseR) - oppositeR));
            var normalG = Math.Round(Math.Sqrt((finalG / inverseG) - oppositeG));
            var normalB = Math.Round(Math.Sqrt((finalB / inverseB) - oppositeB));
            var finalNormal = ((normalR + normalG + normalB) / 60);

            finalNormal = Math.Round(finalNormal / (Math.Sin(4) / 8));

            finalNormal = Math.Round(((Math.Atan2(360, 10) * Math.Atan2(360, 4)) / Math.Atan(5) * -1) * finalNormal, 5);

            return finalNormal;
        }
    }
}