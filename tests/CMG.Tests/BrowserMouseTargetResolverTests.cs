using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserMouseTargetResolverTests
{
    [Fact]
    public void Resolve_UsesCoordinates()
    {
        var point = BrowserMouseTargetResolver.Resolve("debug", new FakeAutomationClient(), Action("mouseMove", [], new Dictionary<string, string> { ["x"] = "12", ["y"] = "24" }));

        Assert.Equal(new ElementPoint(12, 24), point);
    }

    [Fact]
    public void Resolve_UsesAlias()
    {
        var point = BrowserMouseTargetResolver.Resolve("debug", new FakeAutomationClient(), Action("mouseMove", ["center"]));

        Assert.Equal(new ElementPoint(400, 300), point);
    }

    [Fact]
    public void Resolve_RejectsMissingTarget()
    {
        var error = Assert.Throws<ScriptExecutionException>(() =>
            BrowserMouseTargetResolver.Resolve("debug", new FakeAutomationClient(), Action("mouseMove", [])));

        Assert.Contains("requires either one alias", error.Message);
    }

    private static BrowserScriptAction Action(
        string name,
        IReadOnlyList<string> args,
        IReadOnlyDictionary<string, string>? options = null) =>
        new(1, name, name, args, options ?? new Dictionary<string, string>(), []);
}
