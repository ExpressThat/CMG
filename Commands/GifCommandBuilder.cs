using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed class GifCommandBuilder
{
    public Command Build()
    {
        var command = new Command("gif", "GIF artifact inspection and utility commands.");
        command.Subcommands.Add(BuildInspectCommand());
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
}
