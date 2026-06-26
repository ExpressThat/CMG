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

    private static CmgTestCase Test(string name, IReadOnlyDictionary<string, string>? options = null) =>
        new("flow.cmgscript", name, [], options ?? new Dictionary<string, string>());

    private static CmgTestResult Result(string name, bool success) =>
        new(name, "flow.cmgscript", success, [], success ? null : "failed", null, []);
}
