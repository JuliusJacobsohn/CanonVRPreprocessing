using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonDualFisheyeFunctions
{
    public class FisheyeConversionHelper
    {
        /// <summary>
        /// Returns the coordinates in a fisheye circle, for a given set of normalized equirectangular coordinates.
        /// The input and output coordinates are between -1 and 1. The top left corner is (-1, 1)
        /// </summary>
        /// <param name="normalEquirectX"></param>
        /// <param name="normalEquirectY"></param>
        /// <returns></returns>
        public static (double, double) GetCoordinatesInFisheyeImage(double normalEquirectX, double normalEquirectY, int aperture = 180)
        {
            // Convert the aperture to radians.
            double apertureInRadians = Math.PI * aperture / 180.0;

            // Convert normalized equirectangular coordinates to longitude and latitude
            double longitude = normalEquirectX * apertureInRadians / 2;
            double latitude = normalEquirectY * apertureInRadians / 2;

            // Convert longitude and latitude to 3D Cartesian coordinates (px, py, pz)
            double px = Math.Cos(latitude) * Math.Cos(longitude);
            double py = Math.Cos(latitude) * Math.Sin(longitude);
            double pz = Math.Sin(latitude);

            // Calculate theta (angle between the positive X-axis and the vector OP)
            double theta = Math.Acos(px);

            // Calculate phi (angle between the projection of OP onto the YZ plane and the positive Y-axis)
            double phi = Math.Atan2(pz, py);

            double r = 2 * theta / apertureInRadians; //Assuming equidistant projection

            // Convert polar coordinates (r, phi) to Cartesian coordinates (x, y) in the fisheye image
            double x = r * Math.Cos(phi);
            double y = r * Math.Sin(phi);

            // Return the (x, y) coordinates in the fisheye image
            return (x, y);
        }

        /// <summary>
        /// Returns a tensor of size (2, width, height) containing the coordinates in the fisheye image for each pixel in the equirectangular image. [0, :, :] contains the x coordinates, [1, :, :] contains the y coordinates.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="projectionType"></param>
        /// <param name="cached"></param>
        /// <returns></returns>
        public static double[,,] GetFisheyeImage(int width, int height, int aperture = 180, bool overwriteCache = false)
        {
            string fileName = $"FisheyeImage_{width}x{height}.map";
            if (!overwriteCache && File.Exists(fileName))
            {
                return ReadFromFile(fileName);
            }

            Console.WriteLine("Calculating mapping file");
            double[,,] equirectangularImage = new double[2, width, height];

            for (int x = 0; x < equirectangularImage.GetLength(1); x++)
            {
                for (int y = 0; y < equirectangularImage.GetLength(2); y++)
                {
                    // Get the normalized equirectangular coordinates for the current pixel
                    double normalEquirectX = (2.0 * x / equirectangularImage.GetLength(1)) - 1.0;
                    double normalEquirectY = 1.0 - (2.0 * y / equirectangularImage.GetLength(2));

                    // Get the coordinates in the fisheye image for the current pixel
                    (double, double) fisheyeCoordinates = GetCoordinatesInFisheyeImage(normalEquirectX, normalEquirectY, aperture);

                    // Convert the fisheye coordinates to pixel coordinates
                    double fisheyeX = (fisheyeCoordinates.Item1 + 1) / 2;
                    double fisheyeY = (1 - fisheyeCoordinates.Item2) / 2;

                    // Store the coordinates in the texture
                    equirectangularImage[0, x, y] = fisheyeX;
                    equirectangularImage[1, x, y] = fisheyeY;
                }
            }
            WriteToFile(equirectangularImage, fileName);

            return equirectangularImage;
        }

        public static void WriteToFile(double[,,] coordinateMap, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            Console.WriteLine("Writing mapping to file...");
            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                // Write the dimensions of the array
                writer.Write(coordinateMap.GetLength(0));
                writer.Write(coordinateMap.GetLength(1));
                writer.Write(coordinateMap.GetLength(2));

                // Write the tuples to disk
                for (int i = 0; i < coordinateMap.GetLength(0); i++)
                {
                    for (int j = 0; j < coordinateMap.GetLength(1); j++)
                    {
                        for (int k = 0; k < coordinateMap.GetLength(2); k++)
                        {
                            writer.Write(coordinateMap[i, j, k]);
                        }
                    }
                }
            }
            Console.WriteLine("Wrote mapping to file");
        }

        public static double[,,] ReadFromFile(string filename)
        {
            Console.WriteLine("Reading mapping from file...");
            using BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open));
            // Read the dimensions of the array
            int num1 = reader.ReadInt32();
            int num2 = reader.ReadInt32();
            int num3 = reader.ReadInt32();

            // Read the tuples from disk
            double[,,] coordinateLookup = new double[num1, num2, num3];
            for (int i = 0; i < num1; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    for (int k = 0; k < num3; k++)
                    {
                        coordinateLookup[i, j, k] = reader.ReadDouble();
                    }
                }
            }
            Console.WriteLine("Read mapping from file");

            return coordinateLookup;
        }
    }
}
