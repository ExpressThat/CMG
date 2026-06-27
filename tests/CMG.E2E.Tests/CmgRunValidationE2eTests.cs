using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class CmgRunValidationE2eTests : IClassFixture<CmgCliFixture>
{
    private readonly CmgCliFixture fixture;

    public CmgRunValidationE2eTests(CmgCliFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_InvalidConfigJsonFailsBeforeBrowserUse()
    {
        var script = fixture.CreateScript("invalid-config-flow.cmgscript", "test \"x\" { caption \"x\" }");
        var config = fixture.CreateScript("invalid-config.json", "{ nope");

        var result = fixture.Cli.Run("run", script, "--config", config, "--list");

        result.ShouldFail();
        result.StderrContains("was not valid JSON");
        Assert.DoesNotContain("TEST LIST", result.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_InvalidConfigFieldTypeFailsBeforeBrowserUse()
    {
        var script = fixture.CreateScript("invalid-config-type.cmgscript", "test \"x\" { caption \"x\" }");
        var config = fixture.CreateScript("invalid-config-type.json", """{ "retries": "many" }""");

        var result = fixture.Cli.Run("run", script, "--config", config, "--list");

        result.ShouldFail();
        result.StderrContains("Run config option 'retries' must be an integer.");
        Assert.DoesNotContain("TEST LIST", result.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_InvalidShardFailsBeforeBrowserUse()
    {
        var script = fixture.CreateScript("invalid-shard.cmgscript", "test \"x\" { caption \"x\" }");

        var result = fixture.Cli.Run("run", script, "--shard", "2/1", "--list");

        result.ShouldFail();
        result.StderrContains("--shard must use index/count with 1 <= index <= count.");
        Assert.DoesNotContain("TEST LIST", result.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_InvalidBrowserPortFailsBeforeBrowserUse()
    {
        var script = fixture.CreateScript("invalid-port.cmgscript", "test \"x\" { caption \"x\" }");

        var result = fixture.Cli.Run("run", script, "--browser-port", "70000");

        result.ShouldFail();
        result.StderrContains("--browser-port must be between 1 and 65535.");
        Assert.DoesNotContain("TEST ", result.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_NoMatchingFilesReportsPath()
    {
        var missingDirectory = fixture.OutputPath("missing-flows");

        var result = fixture.Cli.Run("run", missingDirectory, "--list");

        result.ShouldFail();
        result.StderrContains($"No CMG script files matched '{missingDirectory}'.");
    }

    [Fact]
    public void RunCommand_DirectScriptReportsMigrationGuidance()
    {
        var script = fixture.CreateScript("direct-script-migration.cmgscript", """
            navigate "https://example.test"
            """);

        var result = fixture.Cli.Run("run", script, "--list");

        result.ShouldFail();
        result.StdoutContains("TEST FAIL direct-script-migration.cmgscript");
        result.StderrContains("requires structured tests");
        result.StderrContains("browser control script --file");
        result.StderrContains("docs/scripting/migration.md");
    }
}
