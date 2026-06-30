using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed class GifCommandBuilder
{
    public Command Build()
    {
        var command = new Command("gif", "GIF artifact inspection and utility commands.");
        command.Subcommands.Add(BuildInspectCommand());
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
}
