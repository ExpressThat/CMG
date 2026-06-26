using System.Net;
using System.Text;

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
            if (test.Annotations.Count > 0)
            {
                builder.AppendLine("    <properties>");
                foreach (var annotation in test.Annotations)
                {
                    builder.AppendLine($"      <property name=\"cmg.annotation.{Encode(annotation.Type)}\" value=\"{Encode(annotation.Description)}\" />");
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
}
