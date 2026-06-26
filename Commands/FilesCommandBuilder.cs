using System.CommandLine;

namespace CMG.Commands;

public sealed class FilesCommandBuilder
{
    public Command Build()
    {
        var command = new Command("files", "Local file utility commands.");
        command.Subcommands.Add(BuildReadCommand());
        command.Subcommands.Add(BuildWriteCommand("write", append: false));
        command.Subcommands.Add(BuildWriteCommand("append", append: true));
        command.Subcommands.Add(BuildExpectCommand());
        return command;
    }

    private static Command BuildReadCommand()
    {
        var path = RequiredPath();
        var encoding = new Option<string?>("--encoding") { Description = "Use base64 for binary output." };
        var command = new Command("read", "Read a local file.") { path, encoding };
        command.SetAction(parseResult =>
        {
            var fullPath = FullPath(parseResult.GetValue(path));
            if (!File.Exists(fullPath))
            {
                Console.Error.WriteLine($"File '{fullPath}' was not found.");
                return 1;
            }

            var value = string.Equals(parseResult.GetValue(encoding), "base64", StringComparison.OrdinalIgnoreCase)
                ? Convert.ToBase64String(File.ReadAllBytes(fullPath))
                : File.ReadAllText(fullPath);
            Console.WriteLine($"FILE_READ 001 {fullPath}");
            Console.WriteLine($"FILE_BODY 001 {value}");
            return 0;
        });
        return command;
    }

    private static Command BuildWriteCommand(string name, bool append)
    {
        var path = RequiredPath();
        var text = new Argument<string>("text") { Description = "Text to write." };
        var command = new Command(name, append ? "Append text to a local file." : "Write text to a local file.") { path, text };
        command.SetAction(parseResult =>
        {
            var fullPath = FullPath(parseResult.GetValue(path));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
            if (append)
            {
                File.AppendAllText(fullPath, parseResult.GetValue(text) ?? string.Empty);
                Console.WriteLine($"FILE_APPENDED 001 {fullPath}");
            }
            else
            {
                File.WriteAllText(fullPath, parseResult.GetValue(text) ?? string.Empty);
                Console.WriteLine($"FILE_WRITTEN 001 {fullPath}");
            }

            return 0;
        });
        return command;
    }

    private static Command BuildExpectCommand()
    {
        var path = RequiredPath();
        var contains = new Option<string?>("--contains") { Description = "Required text in the file." };
        var command = new Command("expect", "Assert that a local file exists and optionally contains text.") { path, contains };
        command.SetAction(parseResult =>
        {
            var fullPath = FullPath(parseResult.GetValue(path));
            if (!File.Exists(fullPath))
            {
                Console.Error.WriteLine($"Expected file '{fullPath}' to exist.");
                return 1;
            }

            var expected = parseResult.GetValue(contains);
            if (!string.IsNullOrEmpty(expected) && !File.ReadAllText(fullPath).Contains(expected, StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Expected file '{fullPath}' to contain '{expected}'.");
                return 1;
            }

            Console.WriteLine($"FILE_OK 001 {fullPath}");
            return 0;
        });
        return command;
    }

    private static Option<FileInfo> RequiredPath() =>
        new("--path")
        {
            Description = "Local file path.",
            Required = true
        };

    private static string FullPath(FileInfo? path) =>
        Path.GetFullPath(path?.FullName ?? string.Empty);
}
