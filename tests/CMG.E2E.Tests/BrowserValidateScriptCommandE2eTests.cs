using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserValidateScriptCommandE2eTests : IClassFixture<CmgCliFixture>
{
    private readonly CmgCliFixture fixture;

    public BrowserValidateScriptCommandE2eTests(CmgCliFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ValidateScript_ReadsValidScriptFromStdinWithoutBrowser()
    {
        var result = fixture.Cli.RunWithInput(
            """
            caption "stdin script"
            set value "ok"
            """,
            "browser",
            "control",
            "validateScript",
            "--file",
            "-");

        result.ShouldPass();
        result.StdoutContains("SCRIPT VALID actions=2");
    }

    [Fact]
    public void ValidateScript_ReportsStdinSyntaxFailure()
    {
        var result = fixture.Cli.RunWithInput(
            "if (true) {\n",
            "browser",
            "control",
            "validateScript",
            "--file",
            "-");

        result.ShouldFail();
        result.StderrContains("missing block close");
    }
}
