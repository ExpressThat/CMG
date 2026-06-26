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

    [Fact]
    public void Plan_ExpandsParameterizedListTests()
    {
        var document = new CmgDslParser().Parse("flow.cmgscript", """
        test.each "opens ${page}" as=page values="profile,checkout" tag=smoke {
          caption "${page}"
        }
        """).Document!;

        var tests = new CmgTestPlanner().Plan(document);

        Assert.Equal(["opens profile", "opens checkout"], tests.Select(test => test.Name));
        Assert.All(tests, test => Assert.Equal("smoke", test.Options["tag"]));
        Assert.Equal(["set", "set", "caption"], tests[0].Actions.Select(action => action.Kind));
        Assert.Equal(["page", "profile"], tests[0].Actions[1].Arguments);
    }

    [Fact]
    public void Plan_TestEachCanUseEachOptionForRows()
    {
        var document = new CmgDslParser().Parse("flow.cmgscript", """
        test.each "case ${item}" each="one,two" {
          caption "${item}"
        }
        """).Document!;

        var tests = new CmgTestPlanner().Plan(document);

        Assert.Equal(["case one", "case two"], tests.Select(test => test.Name));
    }

    [Fact]
    public void Plan_ExpandsParameterizedJsonObjectTests()
    {
        var document = new CmgDslParser().Parse("flow.cmgscript", """
        test.each "opens ${case.name}" as=case json="[{\"name\":\"Profile\",\"selector\":\"#profile\"}]" {
          click "${case.selector}"
        }
        """).Document!;

        var test = Assert.Single(new CmgTestPlanner().Plan(document));

        Assert.Equal("opens Profile", test.Name);
        Assert.Contains(test.Actions, action => action.Kind == "set" && action.Arguments.SequenceEqual(["case.name", "Profile"]));
        Assert.Contains(test.Actions, action => action.Kind == "set" && action.Arguments.SequenceEqual(["case.selector", "#profile"]));
    }

    private static string FirstArgument(CmgNode node) => node.Arguments.FirstOrDefault() ?? node.Name;
}
