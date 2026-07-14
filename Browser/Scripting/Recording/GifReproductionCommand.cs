using System.Text.Json;
using System.Text.Encodings.Web;

namespace CMG.Browser.Scripting.Recording;

public static class GifReproductionCommand
{
    public static string DirectFile(BrowserKind browser, int? port, string file, string gifPath, bool commandLevel) =>
        Direct(browser, port, "--file", Path.GetFullPath(file), gifPath, commandLevel);

    public static string DirectInline(BrowserKind browser, int? port, string script, string gifPath, bool commandLevel) =>
        Direct(browser, port, "--inline", script, gifPath, commandLevel);

    public static string Runner(
        BrowserKind browser,
        int? port,
        string sourcePath,
        string testName,
        string project,
        string gifPath,
        bool commandLevel)
    {
        var parts = new List<string> { "cmg" };
        AddBrowser(parts, browser);
        parts.Add("run");
        parts.Add(Quote(Path.GetFullPath(sourcePath)));
        parts.Add("--grep");
        parts.Add(Quote(testName));
        if (!string.IsNullOrWhiteSpace(project))
        {
            parts.Add("--project");
            parts.Add(Quote(project));
        }
        if (port is int selectedPort)
        {
            parts.Add("--browser-port");
            parts.Add(selectedPort.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        if (commandLevel)
        {
            parts.Add("--gif");
            parts.Add(Quote(Path.GetDirectoryName(Path.GetFullPath(gifPath)) ?? "."));
            AddFormat(parts, gifPath);
        }
        return string.Join(' ', parts);
    }

    public static string Diagnostic(string gifPath, string command) =>
        $"GIF_REPRODUCE path={Quote(Path.GetFullPath(gifPath))} command={Quote(command)}";

    private static string Direct(
        BrowserKind browser,
        int? port,
        string sourceOption,
        string source,
        string gifPath,
        bool commandLevel)
    {
        var parts = new List<string> { "cmg" };
        AddBrowser(parts, browser);
        parts.Add("browser");
        if (port is int selectedPort)
        {
            parts.Add("--port");
            parts.Add(selectedPort.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        parts.AddRange(["control", "script", sourceOption, Quote(source)]);
        if (commandLevel)
        {
            parts.AddRange(["--gif", Quote(Path.GetFullPath(gifPath))]);
            AddFormat(parts, gifPath);
        }
        return string.Join(' ', parts);
    }

    private static void AddFormat(ICollection<string> parts, string path)
    {
        var format = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".apng" => "apng", ".webp" => "webp", ".mp4" => "mp4", _ => null
        };
        if (format is null) return;
        parts.Add("--record-format");
        parts.Add(format);
    }

    private static void AddBrowser(ICollection<string> parts, BrowserKind browser)
    {
        if (browser is BrowserKind.Edge) parts.Add("--edge");
        else if (browser is BrowserKind.Firefox) parts.Add("--firefox");
    }

    private static string Quote(string value) =>
        $"\"{JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping)}\"";
}
