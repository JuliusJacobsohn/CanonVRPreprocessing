using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonDualFisheyeFunctions
{
    public class CanonFisheyeImage : IDisposable
    {
        public Image<Rgba32> RawImage { get; private set; }
        /// <summary>
        /// Right half of the raw image
        /// </summary>
        public Image<Rgba32> LeftSide { get; private set; }
        /// <summary>
        /// Left half of the raw image
        /// </summary>
        public Image<Rgba32> RightSide { get; private set; }
        public CanonFisheyeImage(string path)
        {
            RawImage = Image.Load<Rgba32>(path);

            //Crop image to be 8192x4096
            int x = 0;
            int y = (Constants.CANON_PICTURE_EYE_SOURCE_HEIGHT - Constants.EQUIRECTANGULAR_TARGET_HEIGHT) / 2;

            int width = Constants.CANON_PICTURE_SOURCE_WIDTH;
            int height = Constants.EQUIRECTANGULAR_TARGET_HEIGHT;
            RawImage = RawImage.Clone(i => i.Crop(new Rectangle(x,
                y,
                width,
                height)));
            RightSide = RawImage.Clone(i => i.Crop(new Rectangle(0,
                0,
                Constants.EQUIRECTANGULAR_TARGET_WIDTH,
                Constants.EQUIRECTANGULAR_TARGET_HEIGHT)));
            LeftSide = RawImage.Clone(i => i.Crop(new Rectangle(Constants.EQUIRECTANGULAR_TARGET_WIDTH,
                0,
                Constants.EQUIRECTANGULAR_TARGET_WIDTH,
                Constants.EQUIRECTANGULAR_TARGET_HEIGHT)));
        }

        /// <summary>
        /// Returns the fisheye sphere, given the parameters of the sphere.
        /// </summary>
        /// <returns></returns>
        public Image<Rgba32> GetExactEye(int centerX, int centerY, int radius, bool left)
        {
            int x = centerX - radius;
            int y = centerY - radius;
            int width = radius * 2;
            int height = radius * 2;

            if (left)
            {
                return LeftSide.Clone(i => i.Crop(new Rectangle(x, y, width, height)));
            }
            else
            {
                return RightSide.Clone(i => i.Crop(new Rectangle(x, y, width, height)));
            }
        }

        /// <summary>
        /// Map of [2,width,height] with the fisheye mapping is given
        /// Reads
        /// </summary>
        /// <param name="fisheyeMap"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        public Image<Rgba32> ApplyMap(double[,,] fisheyeMap, int centerX, int centerY, int radius, bool left)
        {
            Image<Rgba32> equirectangularImage = new Image<Rgba32>(Constants.EQUIRECTANGULAR_TARGET_WIDTH, Constants.EQUIRECTANGULAR_TARGET_HEIGHT);
            Image<Rgba32> eye = GetExactEye(centerX, centerY, radius, left);
            for (int x = 0; x < Constants.EQUIRECTANGULAR_TARGET_WIDTH; x++)
            {
                for (int y = 0; y < Constants.EQUIRECTANGULAR_TARGET_HEIGHT; y++)
                {
                    double xMap = fisheyeMap[0, x, y];
                    double yMap = fisheyeMap[1, x, y];

                    double sourceXCoordinate = xMap * eye.Width;
                    double sourceYCoordinate = yMap * eye.Height;

                    //Interpolate between the pixels
                    int targetXPixel = (int)Math.Floor(sourceXCoordinate);
                    double xFactor = sourceXCoordinate - targetXPixel;
                    int targetYPixel = (int)Math.Floor(sourceYCoordinate);
                    double yFactor = sourceYCoordinate - targetYPixel;



                    // Ensure we don't go outside the image boundaries
                    int targetXPixelPlusOne = Math.Min(targetXPixel + 1, eye.Width - 1);
                    int targetYPixelPlusOne = Math.Min(targetYPixel + 1, eye.Height - 1);

                    // Getting the 4 surrounding pixels
                    Rgba32 topLeftPixel = eye[targetXPixel, targetYPixel];
                    Rgba32 topRightPixel = eye[targetXPixelPlusOne, targetYPixel];
                    Rgba32 bottomLeftPixel = eye[targetXPixel, targetYPixelPlusOne];
                    Rgba32 bottomRightPixel = eye[targetXPixelPlusOne, targetYPixelPlusOne];

                    // Interpolating in x direction for each channel
                    byte topRed = Utilities.Lerp(topLeftPixel.R, topRightPixel.R, xFactor);
                    byte topGreen = Utilities.Lerp(topLeftPixel.G, topRightPixel.G, xFactor);
                    byte topBlue = Utilities.Lerp(topLeftPixel.B, topRightPixel.B, xFactor);

                    byte bottomRed = Utilities.Lerp(bottomLeftPixel.R, bottomRightPixel.R, xFactor);
                    byte bottomGreen = Utilities.Lerp(bottomLeftPixel.G, bottomRightPixel.G, xFactor);
                    byte bottomBlue = Utilities.Lerp(bottomLeftPixel.B, bottomRightPixel.B, xFactor);

                    // Interpolating in y direction for the already interpolated x values
                    byte finalRed = Utilities.Lerp(topRed, bottomRed, yFactor);
                    byte finalGreen = Utilities.Lerp(topGreen, bottomGreen, yFactor);
                    byte finalBlue = Utilities.Lerp(topBlue, bottomBlue, yFactor);

                    Rgba32 finalPixel = new Rgba32(finalRed, finalGreen, finalBlue);
                    equirectangularImage[x, y] = finalPixel;
                }
            }

            return equirectangularImage;
        }

        /// <summary>
        /// Applies the fisheye map to both sides of the images and merges the output into a single 8192x4096 image
        /// </summary>
        /// <param name="fisheyeMap"></param>
        /// <param name="leftCenterX"></param>
        /// <param name="leftCenterY"></param>
        /// <param name="leftRadius"></param>
        /// <param name="rightCenterX"></param>
        /// <param name="rightCenterY"></param>
        /// <param name="rightRadius"></param>
        /// <returns></returns>
        public Image<Rgba32> ApplyMap(double[,,] fisheyeMap, int leftCenterX, int leftCenterY, int leftRadius, int rightCenterX, int rightCenterY, int rightRadius)
        {
            Image<Rgba32> leftEye = ApplyMap(fisheyeMap, leftCenterX, leftCenterY, leftRadius, true);
            Image<Rgba32> rightEye = ApplyMap(fisheyeMap, rightCenterX, rightCenterY, rightRadius, false);

            Image<Rgba32> equirectangularImage = new Image<Rgba32>(Constants.FULL_TARGET_WIDTH, Constants.FULL_TARGET_HEIGHT);

            // Directly draw the leftEye on the left half of equirectangularImage
            equirectangularImage.Mutate(ctx => ctx.DrawImage(leftEye, new Point(0, 0), 1));

            // Directly draw the rightEye on the right half of equirectangularImage
            equirectangularImage.Mutate(ctx => ctx.DrawImage(rightEye, new Point(Constants.EQUIRECTANGULAR_TARGET_WIDTH, 0), 1));

            return equirectangularImage;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RawImage.Dispose();
            }
        }
    }
}
