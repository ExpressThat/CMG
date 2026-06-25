using System.Net;
using System.Text;

namespace CMG.Runner;

public static class CmgHtmlReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>CMG Report</title>");
        builder.AppendLine("<style>body{font:14px system-ui;margin:24px} .pass{color:#047857}.fail{color:#b91c1c} pre{background:#f3f4f6;padding:12px;overflow:auto}</style>");
        builder.AppendLine("</head><body><h1>CMG Report</h1>");
        foreach (var test in tests)
        {
            var state = Status(test);
            builder.AppendLine($"<section><h2 class=\"{state}\">{Encode(test.Name)} - {state}</h2>");
            if (!string.IsNullOrWhiteSpace(test.Error))
            {
                builder.AppendLine($"<p>{Encode(test.Error)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(test.GifPath))
            {
                builder.AppendLine($"<p>GIF: {Encode(test.GifPath)}</p>");
            }

            builder.AppendLine("<pre>");
            foreach (var line in test.Output)
            {
                builder.AppendLine(Encode(line));
            }

            builder.AppendLine("</pre></section>");
        }

        builder.AppendLine("</body></html>");
        return builder.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string Status(CmgTestResult test) =>
        string.IsNullOrWhiteSpace(test.Status) ? test.Success ? "pass" : "fail" : test.Status;
}
