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

    private static CmgTestCase Test(string name, IReadOnlyDictionary<string, string>? options = null) =>
        new("flow.cmgscript", name, [], options ?? new Dictionary<string, string>());
}
