using System.Diagnostics;

namespace CMG.Browser;

public interface IBrowserLeaseMonitorLauncher
{
    bool TryStart(BrowserKind browserKind, int port, string ownershipToken, out string? error);
}

public sealed class BrowserLeaseMonitorLauncher : IBrowserLeaseMonitorLauncher
{
    public bool TryStart(BrowserKind browserKind, int port, string ownershipToken, out string? error)
    {
        try
        {
            var info = BuildStartInfo(browserKind, port, ownershipToken);
            using var process = Process.Start(info);
            error = process is null ? "Monitor process did not start." : null;
            return process is not null;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            error = exception.Message;
            return false;
        }
    }

    internal static ProcessStartInfo BuildStartInfo(BrowserKind browserKind, int port, string ownershipToken)
    {
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("CMG process path is unavailable.");
        var info = new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        if (Path.GetFileNameWithoutExtension(processPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            info.ArgumentList.Add(Environment.GetCommandLineArgs()[0]);
        if (browserKind is BrowserKind.Edge) info.ArgumentList.Add("--edge");
        if (browserKind is BrowserKind.Firefox) info.ArgumentList.Add("--firefox");
        foreach (var value in new[] { "browser", "--port", port.ToString(), "lease", "monitor", "--token", ownershipToken })
            info.ArgumentList.Add(value);
        return info;
    }
}
