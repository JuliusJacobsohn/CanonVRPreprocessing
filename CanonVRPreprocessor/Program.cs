using CanonDualFisheyeFunctions;

Console.WriteLine("Enter folder path of images that should be processed");
string? inputPath = Console.ReadLine();

if (inputPath == null)
{
    Console.WriteLine("No input path provided");
    return;
}

Console.WriteLine("Enter folder path where processed images should be saved");
string? outputPath = Console.ReadLine();

if (outputPath == null)
{
    Console.WriteLine("No output path provided");
    return;
}
DateTime startDate = DateTime.Now;

string[] files = Directory.GetFiles(inputPath, "*.jpg");
var mapping = FisheyeConversionHelper.GetFisheyeImage(Constants.EQUIRECTANGULAR_TARGET_WIDTH, Constants.EQUIRECTANGULAR_TARGET_HEIGHT);
Parallel.ForEach(files, new ParallelOptions { /*MaxDegreeOfParallelism = 1*/ }, file =>
{
    string fileName = Path.GetFileName(file);
    Console.WriteLine($"Processing {fileName}");


    string targetPath = Path.Combine(outputPath, fileName);

    CanonFisheyeImage image = new CanonFisheyeImage(file);
    var result = image.ApplyMap(mapping, Constants.LEFT_CIRCLE_CENTER_X, Constants.LEFT_CIRCLE_CENTER_Y, Constants.LEFT_CIRCLE_RADIUS, Constants.RIGHT_CIRCLE_CENTER_X, Constants.RIGHT_CIRCLE_CENTER_Y, Constants.RIGHT_CIRCLE_RADIUS);

    result.Save(targetPath);
});

DateTime endDate = DateTime.Now;
Console.WriteLine($"Processing took {(endDate - startDate).TotalSeconds} seconds");