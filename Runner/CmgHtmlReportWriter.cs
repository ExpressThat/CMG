using System.Net;
using System.Text;

namespace CMG.Runner;

public static class CmgHtmlReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>CMG Report</title>");
        builder.AppendLine("<style>body{font:14px system-ui;margin:24px} .pass{color:#047857}.fail{color:#b91c1c} table{border-collapse:collapse;width:100%;margin-top:12px} th,td{border-bottom:1px solid #e5e7eb;padding:6px;text-align:left;vertical-align:top} code,pre{background:#f3f4f6;padding:2px 4px} pre{padding:8px;overflow:auto;white-space:pre-wrap}</style>");
        builder.AppendLine("</head><body><h1>CMG Report</h1>");
        foreach (var test in tests)
        {
            var state = Status(test);
            builder.AppendLine($"<section><h2 class=\"{state}\">{Encode(test.Name)} - {state}</h2>");
            if (!string.IsNullOrWhiteSpace(test.Project))
            {
                builder.AppendLine($"<p>Project: {Encode(test.Project)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(test.Error))
            {
                builder.AppendLine($"<p>{Encode(test.Error)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(test.GifPath))
            {
                builder.AppendLine($"<p>GIF: {Encode(test.GifPath)}</p>");
            }

            if (test.Annotations.Count > 0)
            {
                builder.AppendLine("<ul>");
                foreach (var annotation in test.Annotations)
                {
                    builder.AppendLine($"<li>{Encode(annotation.Type)}: {Encode(annotation.Description)}</li>");
                }

                builder.AppendLine("</ul>");
            }

            var publicSteps = CmgJsonReportWriter.PublicSteps(test.Steps);
            if (publicSteps.Count > 0)
            {
                builder.AppendLine("<table><thead><tr><th>#</th><th>Line</th><th>Context</th><th>Action</th><th>Status</th><th>Output</th></tr></thead><tbody>");
                foreach (var step in publicSteps)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine($"<td>{step.Sequence}</td>");
                    builder.AppendLine($"<td>{step.LineNumber}</td>");
                    builder.AppendLine($"<td>{Encode(step.Context)}</td>");
                    builder.AppendLine($"<td>{Encode(string.IsNullOrWhiteSpace(step.Action) ? step.Name : step.Action)}</td>");
                    builder.AppendLine($"<td>{(step.Success ? "pass" : "fail")}</td>");
                    builder.AppendLine($"<td><pre>{Encode(string.Join('\n', CleanOutput(step.Output)))}</pre></td>");
                    builder.AppendLine("</tr>");
                }

                builder.AppendLine("</tbody></table>");
            }
            else
            {
                builder.AppendLine("<pre>");
                foreach (var line in CmgJsonReportWriter.PublicOutput(test.Output, test.Steps))
                {
                    builder.AppendLine(Encode(line));
                }

                builder.AppendLine("</pre>");
            }
            builder.AppendLine("</section>");
        }

        builder.AppendLine("</body></html>");
        return builder.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string Status(CmgTestResult test) =>
        string.IsNullOrWhiteSpace(test.Status) ? test.Success ? "pass" : "fail" : test.Status;

    private static IEnumerable<string> CleanOutput(IEnumerable<string> lines) =>
        CmgJsonReportWriter.CleanOutputForReports(lines);
}
