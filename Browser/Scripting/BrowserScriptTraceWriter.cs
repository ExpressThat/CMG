using System.Text.Json;

namespace CMG.Browser.Scripting;

internal static class BrowserScriptTraceWriter
{
    public static void Write(string path, bool success, string? error, IReadOnlyList<BrowserScriptTraceStep> steps)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
        using var stream = File.Create(path);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("type", "cmg-script-trace");
        writer.WriteBoolean("success", success);
        writer.WriteString("error", error);
        writer.WriteStartArray("steps");
        foreach (var step in steps)
        {
            WriteStep(writer, step);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteStep(Utf8JsonWriter writer, BrowserScriptTraceStep step)
    {
        writer.WriteStartObject();
        writer.WriteNumber("sequence", step.Sequence);
        writer.WriteNumber("lineNumber", step.LineNumber);
        writer.WriteString("name", step.Name);
        writer.WriteString("context", step.Context);
        writer.WriteBoolean("success", step.Success);
        writer.WriteString("error", step.Error);
        writer.WriteStartArray("output");
        foreach (var line in step.Output)
        {
            writer.WriteStringValue(line);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
