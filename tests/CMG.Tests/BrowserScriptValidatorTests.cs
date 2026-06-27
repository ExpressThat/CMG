using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptValidatorTests
{
    [Fact]
    public void ValidateText_DirectScriptReportsActions()
    {
        var result = Validator().ValidateText("""
        navigate https://example.test
        click #save
        """);

        Assert.True(result.Success, result.Error);
        Assert.False(result.IsRunner);
        Assert.Equal(2, result.ActionCount);
    }

    [Fact]
    public void ValidateText_RunnerScriptReportsSuitesTestsAndMacros()
    {
        var result = Validator().ValidateText("""
        macro login {
          click #login
        }
        suite "auth" {
          test "sign in" {
            call login
          }
        }
        """);

        Assert.True(result.Success, result.Error);
        Assert.True(result.IsRunner);
        Assert.Equal(1, result.SuiteCount);
        Assert.Equal(1, result.TestCount);
        Assert.Equal(1, result.MacroCount);
    }

    private static BrowserScriptValidator Validator() => new(new BrowserScriptParser());
}
