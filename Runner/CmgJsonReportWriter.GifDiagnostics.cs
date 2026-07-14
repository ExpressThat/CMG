using System.Text.Json;

namespace CMG.Runner;

public static partial class CmgJsonReportWriter
{
    private static void WriteGifDiagnostics(Utf8JsonWriter writer, CmgTestResult test)
    {
        writer.WriteStartArray("gifDiagnostics");
        foreach (var diagnostic in CmgGifDiagnostics.For(test))
        {
            writer.WriteStartObject();
            writer.WriteString("severity", diagnostic.Severity);
            writer.WriteString("message", diagnostic.Message);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}
