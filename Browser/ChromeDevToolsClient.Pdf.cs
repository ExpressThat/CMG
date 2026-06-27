using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            var response = await session.SendCommand("Page.printToPDF", writer =>
            {
                writer.WriteBoolean("landscape", options.Landscape);
                writer.WriteBoolean("printBackground", options.PrintBackground);
                writer.WriteNumber("scale", options.Scale);
                writer.WriteBoolean("preferCSSPageSize", options.PreferCssPageSize);
                if (PdfPaper.TryFormat(options.Format) is { } format)
                {
                    writer.WriteNumber("paperWidth", format.Width);
                    writer.WriteNumber("paperHeight", format.Height);
                }
                WriteInches(writer, "paperWidth", options.Width);
                WriteInches(writer, "paperHeight", options.Height);
                WriteInches(writer, "marginTop", options.MarginTop);
                WriteInches(writer, "marginRight", options.MarginRight);
                WriteInches(writer, "marginBottom", options.MarginBottom);
                WriteInches(writer, "marginLeft", options.MarginLeft);
                if (!string.IsNullOrWhiteSpace(options.PageRanges))
                {
                    writer.WriteString("pageRanges", options.PageRanges);
                }
            });

            if (!TryReadString(response, ["result", "data"], out var data) || string.IsNullOrWhiteSpace(data))
            {
                throw new ChromeDevToolsException("Chrome did not return PDF data.");
            }

            return Convert.FromBase64String(data);
        });
    }

    private static void WriteInches(Utf8JsonWriter writer, string name, string? value)
    {
        if (PdfPaper.Inches(value) is { } inches)
        {
            writer.WriteNumber(name, inches);
        }
    }
}
