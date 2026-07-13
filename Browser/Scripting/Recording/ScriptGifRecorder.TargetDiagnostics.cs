using System.Globalization;
using System.Text.Json;
using CMG.Browser.Scripting;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private readonly List<string> evidenceWarnings = [];

    private void InspectTarget(BrowserScriptAction action, string selector)
    {
        if (remoteDebuggingUrl is null) return;
        try
        {
            var encoded = JsonString(selector);
            var result = devToolsClient.Evaluate(remoteDebuggingUrl, $$"""
                (() => {
                  const nodes = Array.from(document.querySelectorAll({{encoded}}));
                  const rect = nodes[0]?.getBoundingClientRect();
                  return JSON.stringify({
                    count: nodes.length,
                    width: rect?.width ?? 0,
                    height: rect?.height ?? 0,
                    offscreen: !!rect && (rect.bottom <= 0 || rect.right <= 0 || rect.top >= innerHeight || rect.left >= innerWidth)
                  });
                })()
                """);
            using var document = ParseResult(result);
            var root = document.RootElement;
            var count = root.TryGetProperty("count", out var countNode) ? countNode.GetInt32() : 0;
            var width = Number(root, "width");
            var height = Number(root, "height");
            if (count > 1) AddTargetWarning("GIF_WARN_MULTIPLE_TARGETS", action, selector, $"count={count}");
            if (width > 0 && height > 0 && Math.Min(width, height) < 16)
                AddTargetWarning("GIF_WARN_TINY_TARGET", action, selector,
                    $"width={Format(width)} height={Format(height)} threshold=16");
            if (root.TryGetProperty("offscreen", out var offscreen) && offscreen.GetBoolean())
                AddTargetWarning("GIF_WARN_SCROLLED", action, selector, "reason=offscreen-target");
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            // Diagnostics must never make an otherwise valid browser action fail.
        }
    }

    private void RecordNonVisualWarning(GifTimelineStepStart start)
    {
        var action = activeAction;
        if (action is null || frameSink.DurationMilliseconds > start.StartTimeMilliseconds) return;
        if (ConfigurationActions.Contains(action.Name)) return;
        var options = action.Options.Keys.Where(BrowserScriptRunner.IsRecordingOption).ToArray();
        if (options.Length == 0) return;
        evidenceWarnings.Add($"GIF_WARN_NON_VISUAL line={action.LineNumber} action={action.Name} options={string.Join(',', options.OrderBy(value => value))}");
    }

    private void AddTargetWarning(string label, BrowserScriptAction action, string selector, string detail)
    {
        var line = $"{label} line={action.LineNumber} action={action.Name} selector={JsonString(selector)} {detail}";
        if (!evidenceWarnings.Contains(line, StringComparer.Ordinal)) evidenceWarnings.Add(line);
    }

    private static JsonDocument ParseResult(string value)
    {
        var document = JsonDocument.Parse(value);
        if (document.RootElement.ValueKind != JsonValueKind.String) return document;
        var nested = JsonDocument.Parse(document.RootElement.GetString() ?? "{}");
        document.Dispose();
        return nested;
    }

    private static double Number(JsonElement root, string name) =>
        root.TryGetProperty(name, out var node) && node.ValueKind == JsonValueKind.Number ? node.GetDouble() : 0;

    private static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string JsonString(string value) => $"\"{JsonEncodedText.Encode(value)}\"";

    private static readonly HashSet<string> ConfigurationActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "recording", "withRecording", "recordingDefaults", "setRecording", "previewRecordingSettings",
        "pointerStyle", "gif", "recordVideo", "screencast"
    };
}
