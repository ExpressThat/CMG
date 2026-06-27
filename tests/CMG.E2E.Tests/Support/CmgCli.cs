using System.Diagnostics;

namespace CMG.E2E.Tests.Support;

public sealed class CmgCli
{
    private readonly string localAppData;
    private readonly string executable;
    private readonly string? dllFallback;
    private readonly int? browserPort;

    public CmgCli(string root, string localAppData, int? browserPort = null)
    {
        this.localAppData = localAppData;
        this.browserPort = browserPort;
        executable = ResolveExecutable(root);
        dllFallback = executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? executable : null;
    }

    private static string ResolveExecutable(string root)
    {
        var appHost = Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "CMG.exe" : "CMG");
        if (File.Exists(appHost))
        {
            return appHost;
        }

        var outputDll = Path.Combine(AppContext.BaseDirectory, "CMG.dll");
        if (File.Exists(outputDll))
        {
            return outputDll;
        }

        return Path.Combine(root, "bin", "Debug", "net10.0", "CMG.dll");
    }

    public CmgResult Run(params string[] arguments)
    {
        return RunWithTimeout(TimeSpan.FromSeconds(60), arguments);
    }

    public CmgResult RunWithTimeout(TimeSpan timeout, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = dllFallback is null ? executable : "dotnet",
            WorkingDirectory = E2ePaths.RepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        if (dllFallback is not null)
        {
            startInfo.ArgumentList.Add(dllFallback);
        }

        foreach (var argument in BrowserScopedArguments(arguments))
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["LOCALAPPDATA"] = localAppData;
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("dotnet process did not start.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5_000);
            return new CmgResult(-1, Output(stdoutTask), Output(stderrTask), arguments);
        }

        var stdout = stdoutTask.GetAwaiter().GetResult();
        var stderr = stderrTask.GetAwaiter().GetResult();
        return new CmgResult(process.ExitCode, stdout, stderr, arguments);
    }

    private static string Output(Task<string> task) =>
        task.IsCompletedSuccessfully ? task.Result : string.Empty;

    private IEnumerable<string> BrowserScopedArguments(IReadOnlyList<string> arguments)
    {
        if (browserPort is null || arguments.Count is 0)
        {
            return arguments;
        }

        if (arguments[0] == "run" && !arguments.Contains("--browser-port"))
        {
            return arguments.Take(1)
                .Concat(["--browser-port", browserPort.Value.ToString()])
                .Concat(arguments.Skip(1));
        }

        if (arguments[0] != "browser")
        {
            return arguments;
        }

        if (arguments.Count > 1 && arguments[1] == "--port")
        {
            return arguments;
        }

        return arguments.Take(1)
            .Concat(["--port", browserPort.Value.ToString()])
            .Concat(arguments.Skip(1));
    }

    public void KillTrackedBrowser()
    {
        var stateDirectory = Path.Combine(localAppData, "CMG");
        if (!Directory.Exists(stateDirectory))
        {
            return;
        }

        foreach (var stateFile in Directory.EnumerateFiles(stateDirectory, "*.browser.state").Append(Path.Combine(stateDirectory, "browser.state")))
        {
            KillTrackedBrowser(stateFile);
        }
    }

    private static void KillTrackedBrowser(string stateFile)
    {
        if (!File.Exists(stateFile))
        {
            return;
        }

        var processId = File.ReadAllLines(stateFile)
            .Select(line => line.Split('=', 2))
            .FirstOrDefault(parts => parts.Length is 2 && parts[0].Equals("ProcessId", StringComparison.OrdinalIgnoreCase))?[1];
        if (!int.TryParse(processId, out var pid))
        {
            return;
        }

        try
        {
            var process = Process.GetProcessById(pid);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5_000);
            }
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
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
