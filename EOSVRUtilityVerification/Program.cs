using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

public class ImageUtility
{
    public static Image<Rgba32> GenerateGrid(int width, int height, int gridSize = 128)
    {
        Console.WriteLine("Generating grid...");
        var image = new Image<Rgba32>(width, height);
        // Initialize to white
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x % gridSize == 0 || y % gridSize == 0)
                {
                    image[x, y] = new Rgba32(0, 0, 0);
                }
                else
                {
                    image[x, y] = new Rgba32(255, 255, 255);
                }
            }
        }

        return image;
    }
    public static Image<Rgba32> GenerateUniformImage(int width, int height, int r, int g, int b)
    {
        Console.WriteLine("Generating uniform image...");
        var image = new Image<Rgba32>(width, height);
        // Initialize to white
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                image[x, y] = new Rgba32(r, g, b);
            }
        }

        return image;
    }

    public static void CopyPixelValues(ref Image<Rgba32> source, ref Image<Rgba32> target)
    {
        Console.WriteLine("Copying pixel values...");
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                target[x, y] = source[x, y];
            }
        }
    }
}

class Program
{
    const string BASE_PATH = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\EOSVRUtilityVerification";
    const string BASE_PATH_UNCONVERTED = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\EOSVRUtilityVerification\Sample Data";
    const string BASE_PATH_CONVERTED = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\EOSVRUtilityVerification\Sample Data Converted";
    static void Main()
    {
        VerifyFocusInvariance();
        VerifyParallaxInvariance();
        VerifyLensCorrection();
        VerifyLensCircleParameters();
    }

    /// <summary>
    /// Generates two simple black grid on white image files.
    /// One source is closer focused, one source is further focused.
    /// Running the resulting files through EOS VR Utility should result in different output files, if focus plays a role in the conversion process.
    /// Result: The files are the same
    /// </summary>
    static void VerifyFocusInvariance()
    {
        Console.WriteLine("Generating files to verify focus invariance");
        string focusedClosePath = Path.Combine(BASE_PATH_UNCONVERTED, "template_focused_close.jpg");
        string focusedFarPath = Path.Combine(BASE_PATH_UNCONVERTED, "template_focused_far.jpg");
        Image<Rgba32> focusedCloseTemplate = Image.Load<Rgba32>(focusedClosePath);
        Image<Rgba32> focusedFarTemplate = Image.Load<Rgba32>(focusedFarPath);
        string targetPathClose = Path.Combine(BASE_PATH_UNCONVERTED, "target_focus_invariance_close.jpg");
        string targetPathFar = Path.Combine(BASE_PATH_UNCONVERTED, "target_focus_invariance_far.jpg");

        Image<Rgba32> grid = ImageUtility.GenerateGrid(focusedCloseTemplate.Width, focusedCloseTemplate.Height);

        // Copy pixel values from texture to template
        ImageUtility.CopyPixelValues(ref grid, ref focusedCloseTemplate);
        ImageUtility.CopyPixelValues(ref grid, ref focusedFarTemplate);

        focusedCloseTemplate.Save(targetPathClose, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
        focusedFarTemplate.Save(targetPathFar, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
    }

    /// <summary>
    /// Generates two simple black grid on white image files.
    /// Both source images have different parallax.
    /// Running the resulting files through EOS VR Utility should result in different output files, if focus plays a role in the conversion process.
    /// Result: The files are different, parallax actually plays a role in the conversion process.
    /// </summary>
    static void VerifyParallaxInvariance()
    {
        Console.WriteLine("Generating files to verify parallax invariance");
        string focusedClosePath = Path.Combine(BASE_PATH_UNCONVERTED, "template_focused_close.jpg");
        string focusedFarPath = Path.Combine(BASE_PATH_UNCONVERTED, "template_focused_far.jpg");
        Image<Rgba32> focusedCloseTemplate = Image.Load<Rgba32>(focusedClosePath);
        Image<Rgba32> focusedFarTemplate = Image.Load<Rgba32>(focusedFarPath);
        string targetPathClose = Path.Combine(BASE_PATH_UNCONVERTED, "target_parallax_invariance_close.jpg");
        string targetPathFar = Path.Combine(BASE_PATH_UNCONVERTED, "target_parallax_invariance_far.jpg");

        Image<Rgba32> grid = ImageUtility.GenerateGrid(focusedCloseTemplate.Width, focusedCloseTemplate.Height);

        // Copy pixel values from texture to template
        ImageUtility.CopyPixelValues(ref grid, ref focusedCloseTemplate);
        ImageUtility.CopyPixelValues(ref grid, ref focusedFarTemplate);

        focusedCloseTemplate.Save(targetPathClose, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
        focusedFarTemplate.Save(targetPathFar, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
    }

    /// <summary>
    /// Generates two simple images, one black, one white, from the same template image.
    /// Running the resulting files through EOS VR Utility should result in output files that differ from the input files -> lens correction is applied.
    /// Result: The files are the same, lens correction is not applied.
    /// </summary>
    static void VerifyLensCorrection()
    {
        Console.WriteLine("Generating files to verify lens correction");
        string templatePath = Path.Combine(BASE_PATH_UNCONVERTED, "template_focused_close.jpg");
        Image<Rgba32> templateWhite = Image.Load<Rgba32>(templatePath);
        Image<Rgba32> templateBlack = Image.Load<Rgba32>(templatePath);
        string targetPathWhite = Path.Combine(BASE_PATH_UNCONVERTED, "target_lens_correction_white.jpg");
        string targetPathBlack = Path.Combine(BASE_PATH_UNCONVERTED, "target_lens_correction_black.jpg");

        Image<Rgba32> white = ImageUtility.GenerateUniformImage(templateWhite.Width, templateWhite.Height, 255, 255, 255);
        Image<Rgba32> black = ImageUtility.GenerateUniformImage(templateBlack.Width, templateBlack.Height, 0, 0, 0);

        // Copy pixel values from texture to template
        ImageUtility.CopyPixelValues(ref white, ref templateWhite);
        ImageUtility.CopyPixelValues(ref black, ref templateBlack);

        templateWhite.Save(targetPathWhite, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
        templateBlack.Save(targetPathBlack, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
    }

    /// <summary>
    /// Generates a single image with a black circle overlaid on a white background.
    /// Running the resulting files through EOS VR Utility should produce an equirectangular image that is completely black, if the correct lens circle parameters are used (parallax correction = off).
    /// Results for the source images (eyes flipped):
    /// 
    /// Center x = 1957
    /// Center y = 2720
    /// Radius = 1854
    /// 
    /// Center x = 6227
    /// Center y = 2724
    /// Radius = 1856
    /// </summary>
    static void VerifyLensCircleParameters()
    {
        int circle1X = 2048;
        int circle1Y = ((5464 - 4096) / 2) + 2048;
        double radius1 = 3584 / 2;

        int circle2X = 2048 + 4096;
        int circle2Y = ((5464 - 4096) / 2) + 2048;
        double radius2 = 3584 / 2;

        int circle1XOffset = -91;
        int circle1YOffset = -12;
        double radius1Offset = 62; //61.5 works as well

        int circle2XOffset = 83;
        int circle2YOffset = -8;
        double radius2Offset = 64; //64

        circle1X += circle1XOffset;
        circle1Y += circle1YOffset;
        radius1 += radius1Offset;

        circle2X += circle2XOffset;
        circle2Y += circle2YOffset;
        radius2 += radius2Offset;

        Console.WriteLine("Generating file to verify lens circle parameters");
        string templatePath = Path.Combine(BASE_PATH_UNCONVERTED, "template_focused_close.jpg");
        Image<Rgba32> template = Image.Load<Rgba32>(templatePath);
        string targetPath = Path.Combine(BASE_PATH_UNCONVERTED, "circle.jpg");

        for (int x = 0; x < template.Width; x++)
        {
            for (int y = 0; y < template.Height; y++)
            {
                if ((x - circle1X) * (x - circle1X) + (y - circle1Y) * (y - circle1Y) <= radius1 * radius1)
                {
                    template[x, y] = new Rgba32(0, 0, 0);
                }
                if ((x - circle2X) * (x - circle2X) + (y - circle2Y) * (y - circle2Y) <= radius2 * radius2)
                {
                    template[x, y] = new Rgba32(0, 0, 0);
                }
            }
        }

        template.Save(targetPath, new JpegEncoder { Quality = 100, ColorType = JpegEncodingColor.YCbCrRatio444 });
    }
}
