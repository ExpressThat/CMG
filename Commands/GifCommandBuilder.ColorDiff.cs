using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed partial class GifCommandBuilder
{
    private static Command BuildColorDiffCommand()
    {
        var source = new Argument<FileInfo>("source") { Description = "Source PNG frame captured before GIF encoding." };
        var gif = new Argument<FileInfo>("gif") { Description = "Encoded GIF to compare." };
        var frame = new Option<int>("--frame")
        {
            Description = "One-based GIF frame number to compare. Default is 1.",
            DefaultValueFactory = _ => 1
        };
        var command = new Command("color-diff", "Measure color drift from a source PNG to an encoded GIF frame.")
        {
            source, gif, frame
        };
        command.SetAction(parseResult =>
        {
            var sourceFile = parseResult.GetValue(source);
            var gifFile = parseResult.GetValue(gif);
            if (!Exists(sourceFile, "source PNG") || !Exists(gifFile, "GIF")) return 1;
            try
            {
                Console.WriteLine(GifColorComparer.Compare(sourceFile!, gifFile!, parseResult.GetValue(frame)).Format());
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Could not compare GIF color fidelity. {exception.Message}");
                return 1;
            }
        });
        return command;
    }

    private static bool Exists(FileInfo? file, string label)
    {
        if (file?.Exists is true) return true;
        Console.Error.WriteLine($"{label} file '{file?.FullName ?? string.Empty}' was not found.");
        return false;
    }
}
