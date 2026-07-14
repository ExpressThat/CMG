using System.Text.Json;

namespace CMG.Runner;

public static partial class CmgJsonReportWriter
{
    private static void WriteGifReproductionCommands(Utf8JsonWriter writer, CmgTestResult test)
    {
        writer.WriteStartArray("gifReproductionCommands");
        foreach (var item in CmgGifReproductions.For(test))
        {
            writer.WriteStartObject();
            writer.WriteString("gifPath", item.GifPath);
            writer.WriteString("command", item.Command);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}
