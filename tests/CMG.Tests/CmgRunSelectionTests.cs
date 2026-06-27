using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgRunSelectionTests
{
    [Fact]
    public void SelectFocusedTests_ReturnsAllTestsWhenNoOnlyOptionExists()
    {
        var tests = new[] { Test("a"), Test("b") };

        Assert.Equal(tests, CmgRunService.SelectFocusedTests(tests));
    }

    [Fact]
    public void SelectFocusedTests_ReturnsOnlyFocusedTestsWhenAnyOnlyOptionExists()
    {
        var normal = Test("normal");
        var focused = Test("focused", new Dictionary<string, string> { ["only"] = "true" });

        var selected = Assert.Single(CmgRunService.SelectFocusedTests([normal, focused]));

        Assert.Equal("focused", selected.Name);
    }

    [Fact]
    public void HasReachedMaxFailures_StopsWhenThresholdIsReached()
    {
        var tests = new[]
        {
            Result("a", success: false),
            Result("b", success: true),
            Result("c", success: false)
        };

        Assert.False(CmgRunService.HasReachedMaxFailures(tests, 0));
        Assert.True(CmgRunService.HasReachedMaxFailures(tests, 1));
        Assert.True(CmgRunService.HasReachedMaxFailures(tests, 2));
        Assert.False(CmgRunService.HasReachedMaxFailures(tests, 3));
    }

    [Fact]
    public void RepeatTests_AppendsStableRepeatNames()
    {
        var repeated = CmgRunService.RepeatTests([Test("checkout"), Test("profile")], 2);

        Assert.Equal(
            ["checkout [repeat 1/2]", "checkout [repeat 2/2]", "profile [repeat 1/2]", "profile [repeat 2/2]"],
            repeated.Select(test => test.Name).ToArray());
    }

    [Fact]
    public void RepeatTests_ReturnsOriginalScheduleWhenRepeatIsOne()
    {
        var tests = new[] { Test("checkout") };

        Assert.Same(tests, CmgRunService.RepeatTests(tests, 1));
    }

    [Fact]
    public void SelectedTestsForList_AppliesFilterFocusRepeatAndShard()
    {
        var options = Options(grep: "checkout", repeatEach: 2, shardIndex: 2, shardCount: 2);
        var selected = CmgRunService.SelectedTestsForList([
            Test("checkout normal"),
            Test("checkout focused", new Dictionary<string, string> { ["only"] = "true" }),
            Test("profile focused", new Dictionary<string, string> { ["only"] = "true" })
        ], options);

        Assert.Equal(["checkout focused [repeat 2/2]"], selected.Select(test => test.Name).ToArray());
    }

    [Fact]
    public void BuildGifPath_PrefixesProjectName()
    {
        var path = CmgRunService.BuildGifPath(Test("checkout"), Options(projectName: "firefox-smoke") with
        {
            GifDirectory = new DirectoryInfo(Path.GetTempPath())
        });

        Assert.StartsWith("firefox-smoke-checkout", Path.GetFileName(path!.FullName));
    }

    [Fact]
    public void FailedTestErrors_IncludesParseFailuresWithoutDuplicatingStepFailures()
    {
        var errors = CmgRunCommandHandler.FailedTestErrors([
            new CmgTestResult("flow.cmgscript", "flow.cmgscript", false, [], "Line 1: invalid.", null, []),
            new CmgTestResult(
                "checkout",
                "flow.cmgscript",
                false,
                [],
                "Line 3: click failed.",
                null,
                [new CmgStepResult(3, "click", false, [], "Line 3: click failed.", null)])
        ]);

        var error = Assert.Single(errors);
        Assert.Equal("TEST ERROR flow.cmgscript reason=Line 1: invalid.", error);
    }

    private static CmgTestCase Test(string name, IReadOnlyDictionary<string, string>? options = null) =>
        new("flow.cmgscript", name, [], options ?? new Dictionary<string, string>());

    private static CmgTestResult Result(string name, bool success) =>
        new(name, "flow.cmgscript", success, [], success ? null : "failed", null, []);

    private static CmgRunOptions Options(
        string? grep = null,
        string? tag = null,
        int repeatEach = 1,
        int shardIndex = 1,
        int shardCount = 1,
        string projectName = "") =>
        new(
            Browser.BrowserKind.Chrome,
            null,
            null,
            null,
            null,
            null,
            grep,
            tag,
            0,
            0,
            repeatEach,
            false,
            shardIndex,
            shardCount,
            null,
            null,
            null,
            null,
            new Dictionary<string, string>(),
            projectName);
}
