using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerImportTests
{
    [Fact]
    public void Run_ImportsMacrosRelativeToScriptFile()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            var shared = Path.Combine(directory.FullName, "shared.cmgscript");
            File.WriteAllText(shared, """
            macro readTitle {
              evaluate "document.title"
            }
            """);
            var main = Path.Combine(directory.FullName, "main.cmgscript");
            File.WriteAllText(main, """
            import "shared.cmgscript"
            set title {
              call readTitle
            }
            """);
            var client = new FakeAutomationClient();
            client.EvaluateResponses.Enqueue("CMG");

            var result = Runner().Run(main, "debug", client, gif: null);

            Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
            Assert.Contains("SET 004 title CMG", result.StdoutLines);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Run_ImportsWithTabWhitespaceAfterKeyword()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "shared.cmgscript"), """
            macro speak {
              caption "Imported"
            }
            """);
            var main = Path.Combine(directory.FullName, "main.cmgscript");
            File.WriteAllText(main, "import\t\"shared.cmgscript\"\ncall speak");

            var result = Runner().Run(main, "debug", new FakeAutomationClient(), gif: null);

            Assert.True(result.Success, result.Error);
            Assert.Contains("PASS 001 caption Imported", result.StdoutLines);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
