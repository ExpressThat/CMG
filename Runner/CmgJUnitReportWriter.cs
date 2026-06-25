using System.Net;
using System.Text;

namespace CMG.Runner;

public static class CmgJUnitReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var failures = tests.Count(test => !test.Success);
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        builder.AppendLine($"<testsuite name=\"CMG\" tests=\"{tests.Count}\" failures=\"{failures}\">");
        foreach (var test in tests)
        {
            builder.AppendLine($"  <testcase classname=\"CMG\" name=\"{Encode(test.Name)}\">");
            if (!test.Success)
            {
                builder.AppendLine($"    <failure message=\"{Encode(test.Error ?? "Test failed")}\">{Encode(string.Join('\n', test.Output))}</failure>");
            }

            builder.AppendLine("  </testcase>");
        }

        builder.AppendLine("</testsuite>");
        return builder.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
