using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed class GifCommandBuilder
{
    public Command Build()
    {
        var command = new Command("gif", "GIF artifact inspection and utility commands.");
        command.Subcommands.Add(BuildInspectCommand());
        command.Subcommands.Add(BuildCompareCommand());
        command.Subcommands.Add(BuildPresetsCommand());
        return command;
    }

    private static Command BuildInspectCommand()
    {
        var file = new Argument<FileInfo>("file") { Description = "GIF file to inspect." };
        var command = new Command("inspect", "Inspect GIF metadata and palette pressure.") { file };
        command.SetAction(parseResult =>
        {
            var input = parseResult.GetValue(file);
            if (input is null || !input.Exists)
            {
                Console.Error.WriteLine($"GIF file '{input?.FullName ?? string.Empty}' was not found.");
                return 1;
            }

            try
            {
                Console.WriteLine(GifInspector.Inspect(input).Format());
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Could not inspect GIF '{input.FullName}'. {exception.Message}");
                return 1;
            }
        });
        return command;
    }

    private static Command BuildCompareCommand()
    {
        var before = new Argument<FileInfo>("before") { Description = "Baseline GIF file." };
        var after = new Argument<FileInfo>("after") { Description = "Comparison GIF file." };
        var command = new Command("compare", "Compare two GIF artifacts by metadata.") { before, after };
        command.SetAction(parseResult =>
        {
            var beforeFile = parseResult.GetValue(before);
            var afterFile = parseResult.GetValue(after);
            if (!TryInspect(beforeFile, "before", out var beforeGif) ||
                !TryInspect(afterFile, "after", out var afterGif))
            {
                return 1;
            }

            Console.WriteLine(CompareLine(beforeGif, afterGif));
            return 0;
        });
        return command;
    }

    private static bool TryInspect(FileInfo? file, string label, out GifInspection inspection)
    {
        inspection = default!;
        if (file is null || !file.Exists)
        {
            Console.Error.WriteLine($"GIF {label} file '{file?.FullName ?? string.Empty}' was not found.");
            return false;
        }

        try
        {
            inspection = GifInspector.Inspect(file);
            return true;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Could not inspect GIF {label} file '{file.FullName}'. {exception.Message}");
            return false;
        }
    }

    private static string CompareLine(GifInspection before, GifInspection after) =>
        $"GIF_COMPARE before={Quote(before.Path)} after={Quote(after.Path)} " +
        $"framesDelta={after.FrameCount - before.FrameCount} durationMsDelta={after.DurationMilliseconds - before.DurationMilliseconds} " +
        $"sizeBytesDelta={after.SizeBytes - before.SizeBytes} widthDelta={after.Width - before.Width} heightDelta={after.Height - before.Height} " +
        $"sameDimensions={(before.Width == after.Width && before.Height == after.Height).ToString().ToLowerInvariant()} " +
        $"paletteBefore={before.Palette} paletteAfter={after.Palette} paletteColorsBefore={before.PaletteColors} paletteColorsAfter={after.PaletteColors} " +
        $"transparentBefore={before.Transparent.ToString().ToLowerInvariant()} transparentAfter={after.Transparent.ToString().ToLowerInvariant()} " +
        $"repeatBefore={before.RepeatCount} repeatAfter={after.RepeatCount}";

    private static Command BuildPresetsCommand()
    {
        var command = new Command("presets", "List GIF quality, pointer, pulse, and timing presets.");
        command.SetAction(_ =>
        {
            Console.WriteLine("GIF_PRESETS quality=highest,high,medium,low defaultQuality=highest");
            Console.WriteLine("GIF_PRESETS pointerSpeed=slow,normal,fast,instant,multiplier defaultPointerSpeed=normal multiplierExample=1.5x");
            Console.WriteLine("GIF_PRESETS pointerEasing=linear,ease-in,ease-out,ease-in-out,spring defaultPointerEasing=ease-in-out");
            Console.WriteLine("GIF_PRESETS clickPulse=ring,ripple,dot,crosshair,none defaultClickPulse=ring");
            Console.WriteLine($"GIF_PRESETS timing defaultHoldAfterActionMs={ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds} defaultHoldOnFailureMs={ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds}");
            return 0;
        });
        return command;
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
