using System.Diagnostics;

namespace CMG.E2E.Tests.Support;

public sealed class CmgCli
{
    private readonly string root;
    private readonly string localAppData;

    public CmgCli(string root, string localAppData)
    {
        this.root = root;
        this.localAppData = localAppData;
    }

    public CmgResult Run(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(Path.Combine(root, "CMG.csproj"));
        startInfo.ArgumentList.Add("/p:UseSharedCompilation=false");
        startInfo.ArgumentList.Add("--");

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["LOCALAPPDATA"] = localAppData;
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("dotnet process did not start.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(60_000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"CMG command timed out: {string.Join(' ', arguments)}");
        }

        var stdout = stdoutTask.GetAwaiter().GetResult();
        var stderr = stderrTask.GetAwaiter().GetResult();
        return new CmgResult(process.ExitCode, stdout, stderr, arguments);
    }
}

public sealed record CmgResult(int ExitCode, string Stdout, string Stderr, IReadOnlyList<string> Arguments)
{
    public void ShouldPass()
    {
        Assert.True(ExitCode == 0, Message());
    }

    public void ShouldFail()
    {
        Assert.True(ExitCode != 0, Message());
    }

    private string Message() =>
        $"Command: {string.Join(' ', Arguments)}{Environment.NewLine}Exit: {ExitCode}{Environment.NewLine}STDOUT:{Environment.NewLine}{Stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{Stderr}";
}
