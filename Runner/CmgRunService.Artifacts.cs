namespace CMG.Runner;

public sealed partial class CmgRunService
{
    internal static IReadOnlyList<string> ResolveFiles(string path)
    {
        if (File.Exists(path))
        {
            return [Path.GetFullPath(path)];
        }

        return Directory.Exists(path)
            ? Directory.GetFiles(path, "*.cmgscript", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray()
            : [];
    }

    internal static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options)
    {
        if (options.GifDirectory is null)
        {
            return null;
        }

        options.GifDirectory.Create();
        var project = string.IsNullOrWhiteSpace(options.ProjectName) ? string.Empty : $"{SafeName(options.ProjectName)}-";
        var safeName = string.Concat(test.Name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        return new FileInfo(Path.Combine(options.GifDirectory.FullName, $"{project}{safeName}.gif"));
    }

    private static void WriteReports(CmgRunOptions options, IReadOnlyList<CmgTestResult> tests)
    {
        WriteReport(options.JsonReport, CmgJsonReportWriter.Write(tests));
        WriteReport(options.HtmlReport, CmgHtmlReportWriter.Write(tests));
        WriteReport(options.JUnitReport, CmgJUnitReportWriter.Write(tests));
    }

    private static void WriteReport(FileInfo? report, string content)
    {
        if (report is null)
        {
            return;
        }

        report.Directory?.Create();
        File.WriteAllText(report.FullName, content);
    }

    internal static string SafeName(string name) =>
        string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
}
