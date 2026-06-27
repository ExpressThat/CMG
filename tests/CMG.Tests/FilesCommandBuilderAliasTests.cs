using CMG.Commands;

namespace CMG.Tests;

public sealed class FilesCommandBuilderAliasTests
{
    [Fact]
    public void AliasCommands_ReadWriteAppendAndExpectFiles()
    {
        var path = TempPath();
        var command = new FilesCommandBuilder().Build();

        try
        {
            Assert.Equal(0, command.Parse($"writeFile --path \"{path}\" one").Invoke());
            Assert.Equal("one", File.ReadAllText(path));

            Assert.Equal(0, command.Parse($"appendFile --path \"{path}\" \" two\"").Invoke());
            Assert.Equal("one two", File.ReadAllText(path));

            Assert.Equal(0, command.Parse($"expectFile --path \"{path}\" --contains \"one two\"").Invoke());
            Assert.Equal(0, command.Parse($"readFile --path \"{path}\"").Invoke());
            Assert.Equal(0, command.Parse($"fixture --path \"{path}\" --encoding base64").Invoke());
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void ExpectFileAlias_FailsWhenContentIsMissing()
    {
        var path = TempPath();
        File.WriteAllText(path, "actual");

        try
        {
            var exitCode = new FilesCommandBuilder().Build().Parse($"expectFile --path \"{path}\" --contains missing").Invoke();

            Assert.Equal(1, exitCode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
}
