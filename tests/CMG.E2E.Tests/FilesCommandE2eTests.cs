using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class FilesCommandE2eTests : IClassFixture<CmgCliFixture>
{
    private readonly CmgCliFixture fixture;

    public FilesCommandE2eTests(CmgCliFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void FilesCommands_ReadWriteAppendAndExpectRealFiles()
    {
        var file = fixture.OutputPath("files-command.txt");

        var write = fixture.Cli.Run("files", "write", "--path", file, "alpha");
        write.ShouldPass();
        write.StdoutContains("FILE_WRITTEN");

        var append = fixture.Cli.Run("files", "appendFile", "--path", file, "-beta");
        append.ShouldPass();
        append.StdoutContains("FILE_APPENDED");

        var read = fixture.Cli.Run("files", "readFile", "--path", file);
        read.ShouldPass();
        read.StdoutContains("FILE_READ");
        read.StdoutContains("FILE_BODY 001 alpha-beta");

        var expect = fixture.Cli.Run("files", "expectFile", "--path", file, "--contains", "beta");
        expect.ShouldPass();
        expect.StdoutContains("FILE_OK");
    }

    [Fact]
    public void FilesExpect_ExplainsMissingFile()
    {
        var file = fixture.OutputPath("missing-file.txt");
        var result = fixture.Cli.Run("files", "expect", "--path", file);

        result.ShouldFail();
        result.StderrContains("Expected file");
        result.StderrContains("to exist");
    }
}
