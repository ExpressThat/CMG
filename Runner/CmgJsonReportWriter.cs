using System.Text;
using System.Text.Json;

namespace CMG.Runner;

public static class CmgJsonReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var builder = new StringBuilder();
        using var writer = new Utf8JsonWriter(new StringBuilderBuffer(builder), new JsonWriterOptions { Indented = true });
        writer.WriteStartArray();
        foreach (var test in tests)
        {
            writer.WriteStartObject();
            writer.WriteString("name", test.Name);
            writer.WriteString("sourcePath", test.SourcePath);
            writer.WriteString("status", Status(test));
            writer.WriteBoolean("success", test.Success);
            writer.WriteString("error", test.Error);
            writer.WriteString("gifPath", test.GifPath);
            writer.WriteString("tags", test.Tags);
            writer.WriteStartArray("annotations");
            foreach (var annotation in test.Annotations)
            {
                writer.WriteStartObject();
                writer.WriteString("type", annotation.Type);
                writer.WriteString("description", annotation.Description);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteStartArray("output");
            foreach (var line in test.Output)
            {
                writer.WriteStringValue(line);
            }

            writer.WriteEndArray();
            writer.WriteStartArray("steps");
            foreach (var step in test.Steps)
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

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();
        return builder.ToString();
    }

    private static string Status(CmgTestResult test) =>
        string.IsNullOrWhiteSpace(test.Status) ? test.Success ? "passed" : "failed" : test.Status;
}
