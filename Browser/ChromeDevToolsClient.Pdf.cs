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
            });

            if (!TryReadString(response, ["result", "data"], out var data) || string.IsNullOrWhiteSpace(data))
            {
                throw new ChromeDevToolsException("Chrome did not return PDF data.");
            }

            return Convert.FromBase64String(data);
        });
    }
}
