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
            writer.WriteBoolean("success", test.Success);
            writer.WriteString("error", test.Error);
            writer.WriteString("gifPath", test.GifPath);
            writer.WriteStartArray("output");
            foreach (var line in test.Output)
            {
                writer.WriteStringValue(line);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();
        return builder.ToString();
    }
}
