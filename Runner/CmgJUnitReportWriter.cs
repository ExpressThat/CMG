using System.Net;
using System.Text;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public static class CmgJUnitReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var failures = tests.Count(test => !test.Success);
        var skipped = tests.Count(IsSkipped);
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        builder.AppendLine($"<testsuite name=\"CMG\" tests=\"{tests.Count}\" failures=\"{failures}\" skipped=\"{skipped}\">");
        foreach (var test in tests)
        {
            builder.AppendLine($"  <testcase classname=\"CMG\" name=\"{Encode(test.Name)}\">");
            var properties = PropertiesFor(test).ToArray();
            if (properties.Length > 0)
            {
                builder.AppendLine("    <properties>");
                foreach (var property in properties)
                {
                    builder.AppendLine($"      <property name=\"{Encode(property.Name)}\" value=\"{Encode(property.Value)}\" />");
                }

                builder.AppendLine("    </properties>");
            }

            if (IsSkipped(test))
            {
                builder.AppendLine($"    <skipped message=\"{Encode(test.Error ?? "Skipped")}\" />");
            }
            else if (!test.Success)
            {
                builder.AppendLine($"    <failure message=\"{Encode(test.Error ?? "Test failed")}\">{Encode(string.Join('\n', test.Output))}</failure>");
            }

            builder.AppendLine("  </testcase>");
        }

        builder.AppendLine("</testsuite>");
        return builder.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static bool IsSkipped(CmgTestResult test) =>
        test.Status.Equals("skipped", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<JUnitProperty> PropertiesFor(CmgTestResult test)
    {
        if (!string.IsNullOrWhiteSpace(test.Project))
        {
            yield return new("cmg.project", test.Project);
        }

        foreach (var annotation in test.Annotations)
        {
            yield return new($"cmg.annotation.{annotation.Type}", annotation.Description);
        }

        var paths = GifPaths(test.GifPath).ToArray();
        for (var index = 0; index < paths.Length; index++)
        {
            var suffix = paths.Length is 1 ? string.Empty : $".{index + 1}";
            yield return new($"cmg.gif.path{suffix}", paths[index]);
            if (!test.Success && FailureFrameIndex(paths[index]) is int frameIndex)
            {
                yield return new($"cmg.gif.failureFrameIndex{suffix}", frameIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
    }

    private static int? FailureFrameIndex(string path)
    {
        var file = new FileInfo(path);
        if (!file.Exists)
        {
            return null;
        }

        try
        {
            return Math.Max(0, GifInspector.Inspect(file).FrameCount - 1);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IEnumerable<string> GifPaths(string? gifPath) =>
        string.IsNullOrWhiteSpace(gifPath)
            ? []
            : gifPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private sealed record JUnitProperty(string Name, string Value);
}
