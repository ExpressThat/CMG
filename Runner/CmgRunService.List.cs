namespace CMG.Runner;

public sealed partial class CmgRunService
{
    private CmgRunResult ListTests(IReadOnlyList<string> files, CmgRunOptions options)
    {
        var output = new List<string>();
        var listed = new List<CmgTestResult>();
        foreach (var file in files)
        {
            var parse = parser.Parse(file, File.ReadAllText(file));
            if (!parse.Success || parse.Document is null)
            {
                output.Add($"TEST FAIL {Path.GetFileName(file)}");
                listed.Add(new CmgTestResult(Path.GetFileName(file), file, false, [], parse.Error, null, []));
                continue;
            }

            var selected = SelectedTestsForList(planner.Plan(parse.Document), options);
            output.AddRange(GifAuthoringWarnings(selected, options));
            foreach (var test in selected)
            {
                var status = IsSkipped(test) ? "skip" : "run";
                output.Add(ListOutput(status, test.Name, options));
                listed.Add(new CmgTestResult(test.Name, test.SourcePath, true, [], null, null, [])
                {
                    Status = status == "skip" ? "skipped" : "listed",
                    Tags = test.Options.TryGetValue("tag", out var tag) ? tag : string.Empty,
                    Project = options.ProjectName,
                    Annotations = test.Annotations
                });
            }
        }

        return new CmgRunResult(listed.All(test => test.Success), output, listed, null);
    }

    internal static IReadOnlyList<CmgTestCase> SelectedTestsForList(IReadOnlyList<CmgTestCase> planned, CmgRunOptions options)
    {
        var filtered = planned.Where(test => ShouldRun(test, options)).ToArray();
        var focused = SelectFocusedTests(filtered);
        var repeated = RepeatTests(focused, options.RepeatEach);
        return ApplyShard(repeated, options).ToArray();
    }

    private static string ListOutput(string status, string name, CmgRunOptions options) =>
        string.IsNullOrWhiteSpace(options.ProjectName)
            ? $"TEST LIST {status} {name}"
            : $"TEST LIST {status} [{options.ProjectName}] {name}";
}
