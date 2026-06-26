using System.Text.Json;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTraceTests
{
    [Fact]
    public void RunText_StartAndStopTracingWritesStepTrace()
    {
        var directory = TempDirectory();
        var path = Path.Combine(directory.FullName, "trace.json");
        var result = Runner().RunText(
            $"startTracing path=\"{path}\"\ntitle\nstopTracing",
            "debug",
            new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains($"TRACE 003 {path}", result.StdoutLines);
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("cmg-script-trace", json.RootElement.GetProperty("type").GetString());
        Assert.True(json.RootElement.GetProperty("steps").GetArrayLength() >= 2);
    }

    [Fact]
    public void RunText_ActiveTraceWritesFailureTrace()
    {
        var directory = TempDirectory();
        var path = Path.Combine(directory.FullName, "failed-trace.json");
        var result = Runner().RunText(
            $"startTracing path=\"{path}\"\nfail \"boom\"",
            "debug",
            new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains($"TRACE {path}", result.StdoutLines);
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("boom", json.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public void Run_CommandTraceWritesWholeScriptTrace()
    {
        var directory = TempDirectory();
        var script = Path.Combine(directory.FullName, "script.cmgscript");
        var trace = new FileInfo(Path.Combine(directory.FullName, "command-trace.json"));
        File.WriteAllText(script, "title");

        var result = Runner().Run(script, "debug", new FakeAutomationClient(), gif: null, trace);

        Assert.True(result.Success);
        Assert.Contains($"TRACE {trace.FullName}", result.StdoutLines);
        Assert.Contains("\"name\": \"title\"", File.ReadAllText(trace.FullName));
    }

    [Fact]
    public void Run_CommandTraceSuppressesNestedTraceBlocks()
    {
        var directory = TempDirectory();
        var script = Path.Combine(directory.FullName, "nested.cmgscript");
        var trace = new FileInfo(Path.Combine(directory.FullName, "command-trace.json"));
        File.WriteAllText(script, "startTracing path=\"nested.json\"\ntitle\nstopTracing");

        var result = Runner().Run(script, "debug", new FakeAutomationClient(), gif: null, trace);

        Assert.True(result.Success);
        Assert.Contains("TRACE_BLOCK_SUPPRESSED 001", result.StdoutLines);
        Assert.Contains("TRACE_BLOCK_SUPPRESSED 003", result.StdoutLines);
        Assert.Contains($"TRACE {trace.FullName}", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static DirectoryInfo TempDirectory()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "cmg-trace-tests", Guid.NewGuid().ToString("N")));
        directory.Create();
        return directory;
    }
}
