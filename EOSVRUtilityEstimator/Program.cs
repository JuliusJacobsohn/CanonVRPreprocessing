using CanonDualFisheyeFunctions;
using System.Collections.Concurrent;
using static CanonDualFisheyeFunctions.FisheyeConversionHelper;

Console.WriteLine("Enter path to correct refernce image:");
string? correctImagePath = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\CanonVRPreprocessor\Camera Images Processed by EOS VR Utility No Parallax Correction\IMG_0032.JPG";//Console.ReadLine();
if (correctImagePath == null)
{
    Console.WriteLine("No image entered, exiting");
    return;
}

Console.WriteLine("Enter path to sample image of your lens:");
string? sampleImagePath = @"C:\Users\juliu\source\repos\Master-Thesis\CanonVRPreprocessing\CanonVRPreprocessor\Camera Images\IMG_0032.JPG";//Console.ReadLine();
if (sampleImagePath == null)
{
    Console.WriteLine("No image entered, exiting");
    return;
}

Image<Rgba32> correctImage = Image.Load<Rgba32>(correctImagePath);
var correctImageLeft = correctImage.Clone(x => x.Crop(new Rectangle(0, 0, Constants.EQUIRECTANGULAR_TARGET_WIDTH, Constants.EQUIRECTANGULAR_TARGET_HEIGHT)));
var correctImageRight = correctImage.Clone(x => x.Crop(new Rectangle(Constants.EQUIRECTANGULAR_TARGET_WIDTH, 0, Constants.EQUIRECTANGULAR_TARGET_WIDTH, Constants.EQUIRECTANGULAR_TARGET_HEIGHT)));
CanonFisheyeImage sampleImage = new CanonFisheyeImage(sampleImagePath);

double[,,] fisheyeMapping = FisheyeConversionHelper.GetFisheyeImage(Constants.EQUIRECTANGULAR_TARGET_WIDTH, Constants.EQUIRECTANGULAR_TARGET_HEIGHT, 180, true);

List<(int x, int y, int radius)> valuesLeft = new List<(int x, int y, int radius)>();
List<(int x, int y, int radius)> valuesRight = new List<(int x, int y, int radius)>();

int range = 15;

//Fill valuecube
for (int x = Constants.LEFT_CIRCLE_CENTER_X - range; x <= Constants.LEFT_CIRCLE_CENTER_X + range; x++)
{
    for (int y = Constants.LEFT_CIRCLE_CENTER_Y - range; y <= Constants.LEFT_CIRCLE_CENTER_Y + range; y++)
    {
        for (int radius = Constants.LEFT_CIRCLE_RADIUS - range; radius <= Constants.LEFT_CIRCLE_RADIUS + range; radius++)
        {
            valuesLeft.Add((x, y, radius));
        }
    }
}

for (int x = Constants.RIGHT_CIRCLE_CENTER_X - range; x <= Constants.RIGHT_CIRCLE_CENTER_X + range; x++)
{
    for (int y = Constants.RIGHT_CIRCLE_CENTER_Y - range; y <= Constants.RIGHT_CIRCLE_CENTER_Y + range; y++)
    {
        for (int radius = Constants.RIGHT_CIRCLE_RADIUS - range; radius <= Constants.RIGHT_CIRCLE_RADIUS + range; radius++)
        {
            valuesRight.Add((x, y, radius));
        }
    }
}

valuesRight = valuesRight.OrderBy(x => Guid.NewGuid()).ToList();
valuesLeft = valuesLeft.OrderBy(x => Guid.NewGuid()).ToList();

ConcurrentDictionary<(int x, int y, int radius), double> errorsLeft = new ConcurrentDictionary<(int x, int y, int radius), double>();
ConcurrentDictionary<(int x, int y, int radius), double> errorsRight = new ConcurrentDictionary<(int x, int y, int radius), double>();

Console.WriteLine($"Processing {valuesLeft.Count} values for left side");
Console.WriteLine($"Starttime: {DateTime.Now}");
Console.WriteLine();

Parallel.ForEach(valuesLeft, new ParallelOptions { MaxDegreeOfParallelism = 10 }, values =>
{
    //Create image based on mapping
    Image<Rgba32> transformedImage = sampleImage.ApplyMap(fisheyeMapping, values.x, values.y, values.radius, true);

    //Calculate error
    double error = Utilities.CalculateMSE(correctImageLeft, transformedImage);
    errorsLeft.TryAdd(values, error);

    // Protect the console print since it's not thread-safe and multiple tasks could be trying to update at the same time
    lock (Console.Out)
    {
        var bestError = errorsLeft.OrderBy(x => x.Value).First();

        double percentageDone = ((double)errorsLeft.Count / valuesLeft.Count) * 100;
        Console.Write($"\r{DateTime.Now}: Left: {errorsLeft.Count} / {valuesLeft.Count} ({percentageDone.ToString("F2")}%), Best Error: {bestError.Key}: {bestError.Value}");

    }
});
Console.WriteLine();


//Create csv with x, y and error values
using (StreamWriter sw = new StreamWriter("errors_left.csv"))
{
    sw.WriteLine("x;y;radius;error");
    foreach (var error in errorsLeft)
    {
        sw.WriteLine($"{error.Key.x};{error.Key.y};{error.Key.radius};{error.Value}");
    }
}

Console.WriteLine($"Processing {valuesRight.Count} values for right side");
Console.WriteLine($"Starttime: {DateTime.Now}");
Console.WriteLine();

Parallel.ForEach(valuesRight, new ParallelOptions { /*MaxDegreeOfParallelism = 16*/ }, values =>
{
    //Create image based on mapping
    Image<Rgba32> transformedImage = sampleImage.ApplyMap(fisheyeMapping, values.x, values.y, values.radius, false);

    //Calculate error
    double error = Utilities.CalculateMSE(correctImageRight, transformedImage);
    errorsRight.TryAdd(values, error);

    // Protect the console print since it's not thread-safe and multiple tasks could be trying to update at the same time
    lock (Console.Out)
    {
        var bestError = errorsRight.OrderBy(x => x.Value).First();

        double percentageDone = ((double)errorsRight.Count / valuesRight.Count) * 100;
        Console.Write($"\r{DateTime.Now}: Right: {errorsRight.Count} / {valuesRight.Count} ({percentageDone.ToString("F2")}%), Best Error: {bestError.Key}: {bestError.Value}");

    }
});

Console.WriteLine("---");


//Create csv with x, y and error values
using (StreamWriter sw = new StreamWriter("errors_right.csv"))
{
    sw.WriteLine("x;y;radius;error");
    foreach (var error in errorsRight)
    {
        sw.WriteLine($"{error.Key.x};{error.Key.y};{error.Key.radius};{error.Value}");
}
}
//Find minimum error
var minErrorLeft = errorsLeft.OrderBy(x => x.Value).First();
var minErrorRight = errorsRight.OrderBy(x => x.Value).First();

Console.WriteLine($"Left: {minErrorLeft.Key.x}, {minErrorLeft.Key.y}, {minErrorLeft.Key.radius}, {minErrorLeft.Value}");
Console.WriteLine($"Right: {minErrorRight.Key.x}, {minErrorRight.Key.y}, {minErrorRight.Key.radius}, {minErrorRight.Value}");