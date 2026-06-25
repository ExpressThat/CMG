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
}
