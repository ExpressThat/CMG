using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgTestPlannerTests
{
    [Fact]
    public void Plan_AppliesRootAndSuiteHooks()
    {
        var parser = new CmgDslParser();
        var document = parser.Parse("flow.cmgscript", """
        macro rootHelper {
          caption "root macro"
        }
        beforeEach {
          caption "root"
        }
        suite "Suite" {
          macro suiteHelper {
            caption "suite macro"
          }
          beforeEach {
            caption "suite"
          }
          test "case" {
            click "#run"
          }
          afterEach {
            caption "done"
          }
        }
        """).Document!;

        var test = Assert.Single(new CmgTestPlanner().Plan(document));

        Assert.Equal("Suite / case", test.Name);
        Assert.Collection(
            test.Actions,
            action => Assert.Equal("macro", action.Kind),
            action => Assert.Equal("macro", action.Kind),
            action => Assert.Equal("caption", action.Kind),
            action => Assert.Equal("caption", action.Kind),
            action => Assert.Equal("click", action.Kind),
            action => Assert.Equal("caption", action.Kind));
    }

    [Fact]
    public void Plan_SupportsProviderSuiteTestAndOnceHookAliases()
    {
        var document = new CmgDslParser().Parse("flow.cmgscript", """
        before {
          caption "root before"
        }

        describe "Alias suite" {
          before {
            caption "suite before"
          }

          it "case" {
            caption "test"
          }

          specify "other" {
            caption "other"
          }

          after {
            caption "suite after"
          }
        }
        """).Document!;

        var tests = CmgRunService.ApplyOnceHooks(new CmgTestPlanner().Plan(document));

        Assert.Equal(["Alias suite / case", "Alias suite / other"], tests.Select(test => test.Name));
        Assert.Equal(["root before", "suite before", "test"], tests[0].Actions.Select(FirstArgument));
        Assert.Equal(["other", "suite after"], tests[1].Actions.Select(FirstArgument));
    }

    [Fact]
    public void Plan_CascadesSuiteFocusAndSkipOptions()
    {
        var document = new CmgDslParser().Parse("flow.cmgscript", """
        describe.only "Focused suite" tag=smoke slow=4 {
          it "case" only=false {
            caption "run"
          }
          it "fast case" slow=false {
            caption "run"
          }
        }

        context.skip "Legacy" reason="Disabled" {
          test "old" skip=false {
            caption "skip"
          }
        }
        """).Document!;

        var tests = new CmgTestPlanner().Plan(document);

        Assert.Equal("true", tests[0].Options["only"]);
        Assert.Equal("smoke", tests[0].Options["tag"]);
        Assert.Equal("4", tests[0].Options["slow"]);
        Assert.Equal("false", tests[1].Options["slow"]);
        Assert.Equal("true", tests[2].Options["skip"]);
        Assert.Equal("Disabled", tests[2].Options["reason"]);
    }

    private static string FirstArgument(CmgNode node) => node.Arguments.FirstOrDefault() ?? node.Name;
}
