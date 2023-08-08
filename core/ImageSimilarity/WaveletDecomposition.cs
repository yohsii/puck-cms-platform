using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Net;

namespace puck.core.ImageSimilarity
{
    public class WaveletDecomposition
    {
        public static double Sample(Uri fileURL)
        {
            Stream stream = null;
            using (WebClient client = new WebClient())
            {
                stream = client.OpenRead(fileURL);
            }
            return Sample(stream);
        }
        public static double Sample(string filepath)
        {
            var stream = File.OpenRead(filepath);
            return Sample(stream);
        }
        public static double Sample(Stream stream)
        {
            var img = new Bitmap(stream);
            
            double sumCo = 0.0;

            for (int i = 0; i < img.Height; i++)
            {
                var x = i * 2;
                if (x > img.Width - 1) x = img.Width - 1;
                var pix = img.GetPixel(x, i);
                var bright = System.Math.Sqrt((pix.R * pix.R * .241) + (pix.G * pix.G * .691) + (pix.B * pix.B * .068));
                var coefficient = Math.Sin(bright);
                double aspectR = img.VerticalResolution / img.HorizontalResolution;
                var normCo = coefficient / aspectR;
                var inverse = 1 / normCo;
                var finalCo = Math.Round(normCo * inverse);
                sumCo += finalCo;
            }
            //Console.WriteLine(sumCo);
            return sumCo;
        }
    }
}
