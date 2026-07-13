using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed partial class GifCommandBuilder
{
    private static Command BuildTrimCommand()
    {
        var file = new Argument<FileInfo>("file") { Description = "GIF file to trim." };
        var output = new Option<FileInfo?>("--output") { Description = "Trimmed GIF output path." };
        var startFrame = new Option<int?>("--start-frame") { Description = "Inclusive zero-based first frame." };
        var endFrame = new Option<int?>("--end-frame") { Description = "Inclusive zero-based last frame." };
        var startTime = new Option<int?>("--start-time") { Description = "Inclusive start time in milliseconds." };
        var endTime = new Option<int?>("--end-time") { Description = "Exclusive end time in milliseconds." };
        var command = new Command("trim", "Trim a GIF by frame or time range.")
        {
            file, output, startFrame, endFrame, startTime, endTime
        };
        command.SetAction(parseResult =>
        {
            var input = parseResult.GetValue(file);
            var destination = parseResult.GetValue(output);
            if (!Exists(input, "GIF")) return 1;
            if (destination is null) { Console.Error.WriteLine("gif trim requires --output <gif>."); return 1; }
            try
            {
                Console.WriteLine(GifTrimmer.Trim(input!, destination,
                    parseResult.GetValue(startFrame), parseResult.GetValue(endFrame),
                    parseResult.GetValue(startTime), parseResult.GetValue(endTime)).Format());
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Could not trim GIF '{input!.FullName}'. {exception.Message}");
                return 1;
            }
        });
        return command;
    }
}
