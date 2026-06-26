using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            var response = await session.SendCommand("browsingContext.print", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteBoolean("background", options.PrintBackground);
                writer.WriteString("orientation", options.Landscape ? "landscape" : "portrait");
                writer.WriteNumber("scale", options.Scale);
                WriteFirefoxPage(writer, options);
                WriteFirefoxMargins(writer, options);
                WriteFirefoxPageRanges(writer, options.PageRanges);
            });

            return DecodePdf(response);
        });

    private static void WriteFirefoxPage(Utf8JsonWriter writer, PdfPrintOptions options)
    {
        var format = PdfPaper.TryFormat(options.Format);
        var width = PdfPaper.Centimeters(options.Width) ?? format?.Width * 2.54;
        var height = PdfPaper.Centimeters(options.Height) ?? format?.Height * 2.54;
        if (width is null && height is null)
        {
            return;
        }

        writer.WriteStartObject("page");
        WriteNumber(writer, "width", width);
        WriteNumber(writer, "height", height);
        writer.WriteEndObject();
    }

    private static void WriteFirefoxMargins(Utf8JsonWriter writer, PdfPrintOptions options)
    {
        if (options.MarginTop is null && options.MarginRight is null && options.MarginBottom is null && options.MarginLeft is null)
        {
            return;
        }

        writer.WriteStartObject("margin");
        WriteCentimeters(writer, "top", options.MarginTop);
        WriteCentimeters(writer, "right", options.MarginRight);
        WriteCentimeters(writer, "bottom", options.MarginBottom);
        WriteCentimeters(writer, "left", options.MarginLeft);
        writer.WriteEndObject();
    }

    private static void WriteFirefoxPageRanges(Utf8JsonWriter writer, string? pageRanges)
    {
        if (string.IsNullOrWhiteSpace(pageRanges))
        {
            return;
        }

        writer.WriteStartArray("pageRanges");
        foreach (var range in pageRanges.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            writer.WriteStringValue(range);
        }
        writer.WriteEndArray();
    }

    private static void WriteCentimeters(Utf8JsonWriter writer, string name, string? value) =>
        WriteNumber(writer, name, PdfPaper.Centimeters(value));

    private static void WriteNumber(Utf8JsonWriter writer, string name, double? value)
    {
        if (value is not null)
        {
            writer.WriteNumber(name, value.Value);
        }
    }
}
