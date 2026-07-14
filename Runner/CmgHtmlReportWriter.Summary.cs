using System.Text;

namespace CMG.Runner;

public static partial class CmgHtmlReportWriter
{
    private static void WriteRunSummary(StringBuilder builder, IReadOnlyList<CmgTestResult> tests)
    {
        var passed = tests.Count(test => test.Success && !IsSkipped(test));
        var failed = tests.Count(test => !test.Success);
        var skipped = tests.Count(IsSkipped);
        builder.AppendLine("<section class=\"run-summary\"><h2>Run summary</h2>");
        builder.AppendLine($"<p>Total: {tests.Count} | Passed: {passed} | Failed: {failed} | Skipped: {skipped}</p></section>");
    }

    private static void WriteFailureGroups(StringBuilder builder, IReadOnlyList<CmgTestResult> tests)
    {
        var groups = tests.Where(test => !test.Success)
            .GroupBy(FailureReason, StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .ToArray();
        if (groups.Length == 0) return;

        builder.AppendLine("<section class=\"failure-summary\"><h2>Failures by reason</h2>");
        foreach (var group in groups)
        {
            builder.AppendLine($"<h3>{Encode(group.Key)} ({group.Count()})</h3><ul>");
            foreach (var test in group) builder.AppendLine($"<li>{Encode(test.Name)}</li>");
            builder.AppendLine("</ul>");
        }
        builder.AppendLine("</section>");
    }

    private static void WriteRunFilmstrip(StringBuilder builder, IReadOnlyList<CmgTestResult> tests)
    {
        var items = tests.SelectMany(test => GifPaths(test.GifPath).Select(path => (test, path))).ToArray();
        if (items.Length == 0) return;

        builder.AppendLine("<section class=\"run-filmstrip\"><h2>Run filmstrip</h2><div class=\"gif-previews\">");
        foreach (var (test, path) in items)
        {
            var source = Encode(GifSource(path));
            builder.AppendLine("<figure class=\"gif-preview\">");
            builder.AppendLine(RecordingMedia(path, source, Encode($"Visual evidence for {test.Name}")));
            builder.AppendLine($"<figcaption>{Encode(test.Name)} - {Encode(Status(test))}</figcaption></figure>");
        }
        builder.AppendLine("</div></section>");
    }

    private static bool IsSkipped(CmgTestResult test) =>
        test.Status.Equals("skipped", StringComparison.OrdinalIgnoreCase);

    private static string FailureReason(CmgTestResult test) =>
        string.IsNullOrWhiteSpace(test.Error) ? "No failure reason was reported." : test.Error;
}
