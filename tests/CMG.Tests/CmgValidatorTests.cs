using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgValidatorTests
{
    [Fact]
    public void Validate_AllowsCssAndTestIdLocators()
    {
        var test = Test(Node("click", ["css=#open"], []), Node("click", ["testid=save"], []));

        Assert.True(new CmgValidator().Validate(test).Success);
    }

    [Fact]
    public void Validate_AllowsRichLocators()
    {
        var action = new CmgNode(7, "click", "click", ["role=button"], new Dictionary<string, string>(), []);
        var result = new CmgValidator().Validate(Test(action));

        Assert.True(result.Success);
    }

    [Fact]
    public void Validate_DoesNotTreatEvaluateJavaScriptAsLocator()
    {
        var action = new CmgNode(7, "evaluate", "evaluate", ["setTimeout(() => true, 0)"], new Dictionary<string, string>(), []);
        var result = new CmgValidator().Validate(Test(action));

        Assert.True(result.Success);
    }

    private static CmgTestCase Test(params CmgNode[] actions) => new("flow.cmgscript", "flow", actions, new Dictionary<string, string>());

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyList<CmgNode> children) =>
        new(1, kind, args.FirstOrDefault() ?? kind, args, new Dictionary<string, string>(), children);
}
