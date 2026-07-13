using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererCaptionTests
{
    [Fact]
    public void Lower_StepPreservesCaptionOptions()
    {
        var options = new Dictionary<string, string> { ["captionStyle"] = "qa", ["captionPosition"] = "bottom" };
        var action = Node("step", ["Open"], options, [Node("click", ["#open"], [])]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal("step \"Open\" captionStyle=\"qa\" captionPosition=\"bottom\" {", lines[0]);
    }

    [Theory]
    [InlineData("gif")]
    [InlineData("recordVideo")]
    [InlineData("screencast")]
    public void Lower_RecordingArtifactsPreserveBlockAndOptions(string actionName)
    {
        var options = new Dictionary<string, string> { ["quality"] = "archival", ["colors"] = "128" };
        var action = Node(actionName, ["Evidence"], options, [Node("click", ["#open"], [])]);

        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal($"{actionName} \"Evidence\" quality=\"archival\" colors=\"128\" {{", lines[0]);
        Assert.Equal("}", lines[^1]);
        Assert.Contains("click \"#open\"", lines);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyList<CmgNode> children) =>
        new(1, kind, kind, args, new Dictionary<string, string>(), children);

    private static CmgNode Node(
        string kind,
        IReadOnlyList<string> args,
        IReadOnlyDictionary<string, string> options,
        IReadOnlyList<CmgNode> children) =>
        new(1, kind, kind, args, options, children);
}
