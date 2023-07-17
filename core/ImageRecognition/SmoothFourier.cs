﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace puck.core.ImageRecognition
{
    public class Normal {
        public double NormalR { get; set; } = 0;
        public double NormalG { get; set; } = 0;
        public double NormalB { get; set; } = 0;
    }
    public static class SmoothFourier
    {
       
        public static double ImageHueSample(string imageFilePath) {

            var normal = new Normal();
            
            Bitmap img = new Bitmap(imageFilePath);
            double totalR = 0;
            double totalG = 0;
            double totalB = 0;
            double totalBright = (double)0;
            var posstotal = 0;

            for (int i = 0; i < img.Width; i++)
            {
                if(i == 0)
                {
                    posstotal = (img.Width * img.Height) * 255;
                }
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    var r = pixel.R;
                    var g = pixel.G;
                    var b = pixel.B;
                    var bright = System.Math.Sqrt((r * r * .241) + (g *g * .691) + (b *b * .068));
                    totalR += r;
                    totalG += g;
                    totalB += b;
                    totalBright += bright;
                }
                
            }

            var finalR = Math.Round((totalR / posstotal) * totalBright);
            var finalG = Math.Round((totalG / posstotal) * totalBright);
            var finalB = Math.Round((totalB / posstotal) * totalBright);

            var inverseR = Math.Round(1 / finalR, 10);
            var inverseG = Math.Round(1 / finalG, 10);
            var inverseB = Math.Round(1 / finalB, 10);

            var oppositeR = Math.Round(finalR * -1);
            var oppositeG = Math.Round(finalG * -1);
            var oppositeB = Math.Round(finalB * -1);

            var normalR = Math.Round((finalR - inverseR) * oppositeR);
            var normalG = Math.Round((finalG - inverseG) + oppositeG);
            var normalB = Math.Round((finalB - inverseB) + oppositeB);
            var finalNormal = normalR + normalG + normalB;

            return finalNormal;
        } 
    }
}