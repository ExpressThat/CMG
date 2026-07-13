namespace CMG.Browser;

public sealed partial class BrowserController
{
    private static bool IsHeadless(IReadOnlyList<string> arguments) =>
        arguments.Any(argument => argument.Equals("--headless", StringComparison.OrdinalIgnoreCase) ||
            argument.StartsWith("--headless=", StringComparison.OrdinalIgnoreCase));

    private static string? FindExecutable(BrowserKind browserKind) =>
        browserKind switch
        {
            BrowserKind.Edge => EdgeExecutableLocator.Find(),
            BrowserKind.Firefox => FirefoxExecutableLocator.Find(),
            _ => ChromeExecutableLocator.Find()
        };

    private static string GetRemoteDebuggingUrl(BrowserKind browserKind, int remoteDebuggingPort) =>
        browserKind.UsesFirefoxBiDi()
            ? $"ws://127.0.0.1:{remoteDebuggingPort}/session"
            : $"http://127.0.0.1:{remoteDebuggingPort}";

    private static IEnumerable<string> BuildBrowserArguments(
        BrowserKind browserKind,
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments) =>
        browserKind.UsesFirefoxBiDi()
            ? BuildFirefoxArguments(remoteDebuggingPort, userDataDirectory, additionalArguments)
            : BuildChromeArguments(remoteDebuggingPort, userDataDirectory, additionalArguments);

    private static IEnumerable<string> BuildChromeArguments(
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments)
    {
        var arguments = new List<string>
        {
            $"--remote-debugging-port={remoteDebuggingPort}",
            $"--user-data-dir={userDataDirectory}",
            "--no-first-run",
            "--no-default-browser-check"
        };
        arguments.AddRange(additionalArguments);
        if (!additionalArguments.Any(argument => !argument.StartsWith("--", StringComparison.Ordinal)))
            arguments.Add("about:blank");
        return arguments;
    }

    private static IEnumerable<string> BuildFirefoxArguments(
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments)
    {
        var arguments = new List<string>
        {
            "-no-remote",
            "--remote-debugging-port",
            remoteDebuggingPort.ToString(),
            "--profile",
            userDataDirectory
        };
        arguments.AddRange(additionalArguments);
        if (!additionalArguments.Any(argument => !argument.StartsWith("-", StringComparison.Ordinal)))
            arguments.Add("about:blank");
        return arguments;
    }

    private static void WriteBrowserPreferences(BrowserKind browserKind, string userDataDirectory)
    {
        if (!browserKind.UsesFirefoxBiDi()) return;
        File.WriteAllLines(Path.Combine(userDataDirectory, "user.js"), [
            "user_pref(\"remote.active-protocols\", 1);"
        ]);
    }
}
