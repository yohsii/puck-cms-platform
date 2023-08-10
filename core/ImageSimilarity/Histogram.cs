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
    public class ImageSample {
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
        public double Brightness { get; set; }
    }
    public class Histogram
    {
        public static ImageSample Sample(Uri fileURL)
        {
            Stream stream = null;
            using (WebClient client = new WebClient())
            {
                stream = client.OpenRead(fileURL);
            }
            return Sample(stream);
        }
        public static ImageSample Sample(string filepath)
        {
            var stream = File.OpenRead(filepath);
            return Sample(stream);
        }
        public static ImageSample Sample(Stream stream)
        {
            var img = new Bitmap(stream);
            var sample = new ImageSample();
            double possTotal = (img.Width * img.Height) * 255;
            var totalRed = 0.0;
            var totalGreen = 0.0;
		    var totalBlue = 0.0;
		    var totalBright = 0.0;
            var finalBright = 0.0;
            for (int i = 0; i < img.Width; i++)
            {
                for (var j = 0; j < img.Height; j++)
                {
                    var pix = img.GetPixel(i, j);
                    var bright = System.Math.Sqrt((pix.R * pix.R * .241) + (pix.G * pix.G * .691) + (pix.B * pix.B * .068));
                    totalRed += pix.R;
                    totalGreen += pix.G;
                    totalBlue += pix.B;
                    totalBright += bright;
                }
            }
            totalRed = Math.Round((totalRed / possTotal)*255);
            totalGreen = Math.Round((totalGreen / possTotal) * 255);
            totalBlue = Math.Round((totalBlue/possTotal)*255);
            finalBright = Math.Round((totalBright/possTotal)*100);
            sample.Red = totalRed;
            sample.Green = totalGreen;
            sample.Blue = totalBlue;
            sample.Brightness = finalBright;
            return sample;
        }
    }
}
