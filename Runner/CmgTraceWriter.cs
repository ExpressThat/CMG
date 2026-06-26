using System.Text;
using System.Text.Json;

namespace CMG.Runner;

public static class CmgTraceWriter
{
    public static void Write(DirectoryInfo? directory, IReadOnlyList<CmgTestResult> tests)
    {
        if (directory is null)
        {
            return;
        }

        directory.Create();
        foreach (var test in tests)
        {
            var project = string.IsNullOrWhiteSpace(test.Project) ? string.Empty : $"{SafeName(test.Project)}-";
            var file = Path.Combine(directory.FullName, $"{project}{SafeName(test.Name)}.trace.json");
            File.WriteAllText(file, WriteTrace(test));
        }
    }

    private static string WriteTrace(CmgTestResult test)
    {
        var builder = new StringBuilder();
        using var writer = new Utf8JsonWriter(new StringBuilderBuffer(builder), new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("name", test.Name);
        writer.WriteString("project", test.Project);
        writer.WriteString("sourcePath", test.SourcePath);
        writer.WriteBoolean("success", test.Success);
        writer.WriteString("error", test.Error);
        writer.WriteString("gifPath", test.GifPath);
        writer.WriteStartArray("steps");
        foreach (var step in test.Steps)
        {
            WriteStep(writer, step);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return builder.ToString();
    }

    private static void WriteStep(Utf8JsonWriter writer, CmgStepResult step)
    {
        writer.WriteStartObject();
        writer.WriteNumber("lineNumber", step.LineNumber);
        writer.WriteString("name", step.Name);
        writer.WriteBoolean("success", step.Success);
        writer.WriteString("error", step.Error);
        writer.WriteString("gifPath", step.GifPath);
        writer.WriteStartArray("output");
        foreach (var line in step.Output)
        {
            writer.WriteStringValue(line);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static string SafeName(string name) =>
        string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
}
