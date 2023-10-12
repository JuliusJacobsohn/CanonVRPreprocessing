using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonDualFisheyeFunctions
{
    public static class Utilities
    {
        /// <summary>
        /// Calculates the mean squared error between two images.
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <returns></returns>
        public static double CalculateMSE(this Image<Rgba32> image1, Image<Rgba32> image2)
        {
            if (image1.Width != image2.Width || image1.Height != image2.Height)
            {
                throw new ArgumentException("Images must be the same size");
            }

            double sum = 0;
            for (int x = 0; x < image1.Width; x++)
            {
                for (int y = 0; y < image1.Height; y++)
                {
                    var pixel1 = image1[x, y];
                    var pixel2 = image2[x, y];

                    sum += Math.Pow(pixel1.R - pixel2.R, 2);
                    sum += Math.Pow(pixel1.G - pixel2.G, 2);
                    sum += Math.Pow(pixel1.B - pixel2.B, 2);
                }
            }

            return sum / (image1.Width * image1.Height * 3);

        }

        public static byte Lerp(byte start, byte end, double weight)
        {
            return (byte)((1 - weight) * start + weight * end);
        }
    }
}
