using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgOnceHookTests
{
    [Fact]
    public void ApplyOnceHooks_InsertsRootAndSuiteHooksAroundRunnableScope()
    {
        var document = new CmgDslParser().Parse("flow.cmgscript", """
        beforeAll { caption "root before all" }
        afterAll { caption "root after all" }
        suite "Suite" {
          beforeAll { caption "suite before all" }
          afterAll { caption "suite after all" }
          beforeEach { caption "before each" }
          test "one" { click "#one" }
          test "two" { click "#two" }
        }
        """).Document!;

        var tests = CmgRunService.ApplyOnceHooks(new CmgTestPlanner().Plan(document));

        Assert.Equal(
            ["root before all", "suite before all", "before each", "#one"],
            tests[0].Actions.Select(FirstArgument).ToArray());
        Assert.Equal(
            ["before each", "#two", "suite after all", "root after all"],
            tests[1].Actions.Select(FirstArgument).ToArray());
    }

    [Fact]
    public void ApplyOnceHooks_IgnoresSkippedTestsWhenChoosingHookTargets()
    {
        var first = Test("skip", skip: true);
        var second = Test("run");
        var tests = CmgRunService.ApplyOnceHooks([first, second]);

        Assert.Equal(["click"], tests[0].Actions.Select(action => action.Kind).ToArray());
        Assert.Equal(["before", "click", "after"], tests[1].Actions.Select(FirstArgument).ToArray());
    }

    private static CmgTestCase Test(string name, bool skip = false) =>
        new("flow.cmgscript", name, [Node("click")], skip ? new Dictionary<string, string> { ["skip"] = "true" } : new Dictionary<string, string>())
        {
            RootBeforeAll = [Node("caption", "before")],
            RootAfterAll = [Node("caption", "after")]
        };

    private static CmgNode Node(string kind, string? argument = null) =>
        new(1, kind, kind, argument is null ? [] : [argument], new Dictionary<string, string>(), []);

    private static string FirstArgument(CmgNode action) => action.Arguments.FirstOrDefault() ?? action.Kind;
}
