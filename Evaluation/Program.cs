using CanonDualFisheyeFunctions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

internal class Program
{
    private static void Main(string[] args)
    {
        //CreateAnaglyph(@"C:\Users\juliu\source\repos\Master-Thesis\Raytracer\build\src\scene2_4096_stereo\texture\scene2_4096_stereo_0.png", @"C:\Users\juliu\source\repos\Master-Thesis\Thesis\figures\evaluation\parallax_anaglyph.png");
        //CreateAnaglyph(@"C:\Users\juliu\source\repos\Master-Thesis\Raytracer\build\src\scene2_4096_stereo\texture\scene2_4096_stereo_55.png", @"C:\Users\juliu\source\repos\Master-Thesis\Thesis\figures\evaluation\parallax_anaglyph_topleft.png");
        //MeasureMSE();
    }

    public static void CreateAnaglyph(string inputPath, string outputPath)
    {
        using (Image<Rgba32> fullImage = Image.Load<Rgba32>(inputPath))
        {
            int halfWidth = fullImage.Width / 2;
            int height = fullImage.Height;

            var leftImage = fullImage.Clone(ctx => ctx.Crop(new Rectangle(0, 0, halfWidth, height)));
            var rightImage = fullImage.Clone(ctx => ctx.Crop(new Rectangle(halfWidth, 0, halfWidth, height)));

            var anaglyph = new Image<Rgba32>(halfWidth, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < halfWidth; x++)
                {
                    Rgba32 leftPixel = leftImage[x, y];
                    Rgba32 rightPixel = rightImage[x, y];

                    anaglyph[x, y] = new Rgba32(leftPixel.R, rightPixel.G, rightPixel.B, 255);
                }
            }

            anaglyph.Save(outputPath);
        }
    }

    static void MeasureMSE()
    {
        string pathOwn = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\CanonVRPreprocessor\Camera Images Processed by Own Tool";
        string pathOwnOneOff = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\CanonVRPreprocessor\Camera Images Processed by Own Tool One Off";
        string pathCanon = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\CanonVRPreprocessor\Camera Images Processed by EOS VR Utility No Parallax Correction";

        double sum1 = 0;

        //Iterate over one folder, get file with the same name from both folders, calculate MSE
        string[] files = System.IO.Directory.GetFiles(pathOwn, "*.jpg");
        foreach(var targetPathOwn in files)
        {
            string fileName = System.IO.Path.GetFileName(targetPathOwn);
            string targetPathCanon = System.IO.Path.Combine(pathCanon, fileName);

            Image<Rgba32> ownImage = Image.Load<Rgba32>(targetPathOwn);
            Image<Rgba32> canonImage = Image.Load<Rgba32>(targetPathCanon);

            double mse = Utilities.CalculateMSE(ownImage, canonImage);
            sum1 += mse;
            Console.WriteLine($"MSE for {fileName}: {mse}");
        };

        double sum2 = 0;
        string[] files2 = System.IO.Directory.GetFiles(pathOwnOneOff, "*.jpg");
        foreach (var targetPathOwn in files2)
        {
            string fileName = System.IO.Path.GetFileName(targetPathOwn);
            string targetPathCanon = System.IO.Path.Combine(pathCanon, fileName);

            Image<Rgba32> ownImage = Image.Load<Rgba32>(targetPathOwn);
            Image<Rgba32> canonImage = Image.Load<Rgba32>(targetPathCanon);

            double mse = Utilities.CalculateMSE(ownImage, canonImage);
            sum2 += mse;
            Console.WriteLine($"MSE for {fileName}: {mse}");
        };

        //Print averages
        Console.WriteLine($"Average MSE for own tool: {sum1 / files.Length}");
        Console.WriteLine($"Average MSE for own tool one off: {sum2 / files.Length}");
    }

    static void GenerateCircleImage()
    {
        int imageSize = 4000;
        int lineThickness = 11;
        int step = imageSize / 10;

        using (Image<Rgba32> image = new Image<Rgba32>(imageSize, imageSize))
        {
            float radius = imageSize / 2f;
            PointF center = new PointF(radius, radius);

            // Set image to white
            image.Mutate(ctx => ctx.Fill(Color.White));

            for (int i = 0; i < 10; i++)
            {
                int position = i * step;
                int halfThickness = lineThickness / 2;
                float yPos = position + halfThickness;
                float xPos = position + halfThickness;

                float dY = (float)Math.Sqrt(radius * radius - (yPos - radius) * (yPos - radius));
                float dX = (float)Math.Sqrt(radius * radius - (xPos - radius) * (xPos - radius));

                // Draw horizontal lines within circle
                image.Mutate(ctx => ctx.DrawLine(Color.Black, lineThickness, new PointF(center.X - dY, yPos), new PointF(center.X + dY, yPos)));

                // Draw vertical lines within circle
                image.Mutate(ctx => ctx.DrawLine(Color.Black, lineThickness, new PointF(xPos, center.Y - dX), new PointF(xPos, center.Y + dX)));
            }

            // Draw red circle
            image.Mutate(ctx => ctx.Draw(Color.Red, lineThickness, new EllipsePolygon(center.X, center.Y, 2 * radius - lineThickness / 2f, 2 * radius - lineThickness / 2f)));

            // Save the image
            image.Save("grid_image.png");

            var fisheyeMap = FisheyeConversionHelper.GetFisheyeImage(imageSize, imageSize);

            Image<Rgba32> equirectangularImage = new Image<Rgba32>(imageSize, imageSize);
            for (int x = 0; x < imageSize; x++)
            {
                for (int y = 0; y < imageSize; y++)
                {
                    double xMap = fisheyeMap[0, x, y];
                    double yMap = fisheyeMap[1, x, y];

                    double sourceXCoordinate = xMap * image.Width;
                    double sourceYCoordinate = yMap * image.Height;

                    //Interpolate between the pixels
                    int targetXPixel = (int)Math.Floor(sourceXCoordinate);
                    double xFactor = sourceXCoordinate - targetXPixel;
                    int targetYPixel = (int)Math.Floor(sourceYCoordinate);
                    double yFactor = sourceYCoordinate - targetYPixel;



                    // Ensure we don't go outside the image boundaries
                    int targetXPixelPlusOne = Math.Min(targetXPixel + 1, image.Width - 1);
                    int targetYPixelPlusOne = Math.Min(targetYPixel + 1, image.Height - 1);

                    // Getting the 4 surrounding pixels
                    Rgba32 topLeftPixel = image[targetXPixel, targetYPixel];
                    Rgba32 topRightPixel = image[targetXPixelPlusOne, targetYPixel];
                    Rgba32 bottomLeftPixel = image[targetXPixel, targetYPixelPlusOne];
                    Rgba32 bottomRightPixel = image[targetXPixelPlusOne, targetYPixelPlusOne];

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

            equirectangularImage.Save("equirectangular_image.png");
        }
    }

    static void GenerateStereoImage()
    {
        var x = new CanonFisheyeImage(@"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\CanonVRPreprocessor\Camera Images\IMG_0034.JPG");

        var leftEye = x.GetExactEye(Constants.LEFT_CIRCLE_CENTER_X, Constants.LEFT_CIRCLE_CENTER_Y, Constants.LEFT_CIRCLE_RADIUS, true);
        var rightEye = x.GetExactEye(Constants.RIGHT_CIRCLE_CENTER_X, Constants.RIGHT_CIRCLE_CENTER_Y, Constants.RIGHT_CIRCLE_RADIUS, false);

        leftEye.Mutate(ctx => ctx.Draw(Color.Red, 10, new EllipsePolygon(leftEye.Width / 2f, leftEye.Height / 2f, leftEye.Width - 5, leftEye.Height - 5)));
        rightEye.Mutate(ctx => ctx.Draw(Color.Red, 10, new EllipsePolygon(rightEye.Width / 2f, rightEye.Height / 2f, rightEye.Width - 5, rightEye.Height - 5)));



        // Create new image to hold both
        int newWidth = leftEye.Width + rightEye.Width;
        int newHeight = Math.Max(leftEye.Height, rightEye.Height);

        using var output = new Image<Rgba32>(new Configuration(), newWidth, newHeight);

        // Copy images
        output.Mutate(ctx =>
        {
            ctx.DrawImage(leftEye, new Point(0, 0), 1);
            ctx.DrawImage(rightEye, new Point(leftEye.Width, 0), 1);
        });
        output.Save("stereoImage.png", new PngEncoder());
    }
}