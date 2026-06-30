using CMG.Browser.Scripting;

namespace CMG.Browser;

public sealed partial class BrowserControlCommandHandler
{
    private static bool ValidateBrowserSelection(BrowserKind browserKind)
    {
        if (browserKind is not BrowserKind.InvalidSelection)
        {
            return true;
        }

        Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
        return false;
    }

    private static bool ValidatePort(int? port)
    {
        if (port is null || port is >= 1 and <= 65535)
        {
            return true;
        }

        Console.Error.WriteLine("--port must be between 1 and 65535.");
        return false;
    }

    private static int WriteScriptResult(ScriptRunResult result)
    {
        foreach (var line in result.StdoutLines)
        {
            Console.WriteLine(line);
        }

        if (!result.Success && !result.Skipped && !string.IsNullOrWhiteSpace(result.Error))
        {
            Console.Error.WriteLine(result.Error);
        }

        return result.Success || result.Skipped ? 0 : 1;
    }
}
