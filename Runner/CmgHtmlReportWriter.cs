using System.Net;
using System.Text;

namespace CMG.Runner;

public static partial class CmgHtmlReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>CMG Report</title>");
        builder.AppendLine("<style>body{font:14px system-ui;margin:24px} .pass{color:#047857}.fail{color:#b91c1c} table{border-collapse:collapse;width:100%;margin-top:12px} th,td{border-bottom:1px solid #e5e7eb;padding:6px;text-align:left;vertical-align:top} code,pre{background:#f3f4f6;padding:2px 4px} pre{padding:8px;overflow:auto;white-space:pre-wrap}.gif-previews,.gif-evidence{display:flex;flex-wrap:wrap;gap:12px;margin:8px 0}.gif-preview,.evidence-frame{margin:0}.gif-preview img,.gif-preview video,.evidence-frame img{display:block;max-width:360px;max-height:240px;border:1px solid #e5e7eb}.gif-preview figcaption,.evidence-frame figcaption{font-size:12px;color:#4b5563;max-width:360px;overflow-wrap:anywhere}.failure-frame img{border-color:#b91c1c}.evidence-links{white-space:nowrap}</style>");
        builder.AppendLine("</head><body><h1>CMG Report</h1>");
        WriteRunSummary(builder, tests);
        WriteFailureGroups(builder, tests);
        WriteRunFilmstrip(builder, tests);
        var testIndex = 0;
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

            WriteGifDiagnostics(builder, test);

            WriteGifPreviews(builder, test);
            WriteFailureFrame(builder, test, testIndex);

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
                builder.AppendLine("<table><thead><tr><th>#</th><th>Line</th><th>Context</th><th>Action</th><th>Status</th><th>Evidence</th><th>Output</th></tr></thead><tbody>");
                foreach (var step in publicSteps)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine($"<td>{step.Sequence}</td>");
                    builder.AppendLine($"<td>{step.LineNumber}</td>");
                    builder.AppendLine($"<td>{Encode(step.Context)}</td>");
                    builder.AppendLine($"<td>{Encode(string.IsNullOrWhiteSpace(step.Action) ? step.Name : step.Action)}</td>");
                    builder.AppendLine($"<td>{(step.Success ? "pass" : "fail")}</td>");
                    WriteEvidenceLinks(builder, step, testIndex);
                    builder.AppendLine($"<td><pre>{Encode(string.Join('\n', CleanOutput(step.Output)))}</pre></td>");
                    builder.AppendLine("</tr>");
                }

                builder.AppendLine("</tbody></table>");
                WriteEvidenceGallery(builder, publicSteps, testIndex);
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
            testIndex++;
        }

        builder.AppendLine("</body></html>");
        return builder.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static void WriteGifDiagnostics(StringBuilder builder, CmgTestResult test)
    {
        var diagnostics = CmgGifDiagnostics.For(test);
        if (diagnostics.Count == 0) return;
        builder.AppendLine("<section class=\"gif-diagnostics\"><h3>GIF diagnostics</h3><ul>");
        foreach (var diagnostic in diagnostics)
            builder.AppendLine($"<li class=\"{Encode(diagnostic.Severity)}\">{Encode(diagnostic.Message)}</li>");
        builder.AppendLine("</ul></section>");
    }

    private static void WriteGifPreviews(StringBuilder builder, CmgTestResult test)
    {
        var paths = GifPaths(test.GifPath).ToArray();
        if (paths.Length is 0)
        {
            return;
        }

        builder.AppendLine("<div class=\"gif-previews\">");
        var reproduction = CmgGifReproductions.For(test)
            .ToDictionary(item => Path.GetFullPath(item.GifPath), item => item.Command, StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            var source = Encode(GifSource(path));
            var label = Encode(path);
            var review = CmgGifReviewMetadataReader.Read(test, path);
            var alt = Encode(review.AltText ?? $"Visual evidence for {test.Name}");
            builder.AppendLine("<figure class=\"gif-preview\">");
            builder.AppendLine(RecordingMedia(path, source, alt));
            builder.AppendLine($"<figcaption>{RecordingLabel(path)}: {label}</figcaption>");
            if (!string.IsNullOrWhiteSpace(review.Description))
                builder.AppendLine($"<figcaption>Description: {Encode(review.Description)}</figcaption>");
            if (!string.IsNullOrWhiteSpace(review.NarrationPath))
                builder.AppendLine($"<figcaption><a href=\"{Encode(GifSource(review.NarrationPath))}\">Screen-reader narration</a></figcaption>");
            if (reproduction.TryGetValue(Path.GetFullPath(path), out var command))
                builder.AppendLine($"<figcaption>Reproduce: <code>{Encode(command)}</code></figcaption>");
            builder.AppendLine("</figure>");
        }

        builder.AppendLine("</div>");
    }

    private static IEnumerable<string> GifPaths(string? gifPath)
    {
        if (string.IsNullOrWhiteSpace(gifPath))
        {
            yield break;
        }

        foreach (var path in gifPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return path;
        }
    }

    private static string GifSource(string path) =>
        Path.IsPathRooted(path)
            ? new Uri(Path.GetFullPath(path)).AbsoluteUri
            : path.Replace('\\', '/');

    private static string Status(CmgTestResult test) =>
        string.IsNullOrWhiteSpace(test.Status) ? test.Success ? "pass" : "fail" : test.Status;

    private static IEnumerable<string> CleanOutput(IEnumerable<string> lines) =>
        CmgJsonReportWriter.CleanOutputForReports(lines);
}
