using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CMG.Browser.Scripting;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private static readonly Dictionary<string, List<string>> InitScripts = [];
    private static readonly object InitScriptLock = new();

    public string AddInitScript(string remoteDebuggingUrl, string source)
    {
        lock (InitScriptLock)
        {
            var key = remoteDebuggingUrl.TrimEnd('/');
            var scripts = InitScripts.GetValueOrDefault(key) ?? [];
            scripts.Add(source);
            InitScripts[key] = scripts;
            return scripts.Count.ToString(CultureInfo.InvariantCulture);
        }
    }

    public void ArmDiagnostics(string remoteDebuggingUrl)
    {
        var source = BrowserConsoleScripts.InstallDiagnostics();
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await session.SendCommand("Page.addScriptToEvaluateOnNewDocument", writer =>
            {
                writer.WriteString("source", source);
            });
            await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", source);
                writer.WriteBoolean("returnByValue", true);
                writer.WriteBoolean("awaitPromise", true);
            });
            return true;
        });
    }

    private static async Task ApplyInitScripts(DevToolsSession session, string remoteDebuggingUrl)
    {
        foreach (var source in SnapshotInitScripts(remoteDebuggingUrl))
        {
            var response = await session.SendCommand("Page.addScriptToEvaluateOnNewDocument", writer =>
            {
                writer.WriteString("source", source);
            });
            _ = response;
        }
    }

    private static IReadOnlyList<string> SnapshotInitScripts(string remoteDebuggingUrl)
    {
        lock (InitScriptLock)
        {
            return InitScripts.GetValueOrDefault(remoteDebuggingUrl.TrimEnd('/'))?.ToArray() ?? [];
        }
    }
}
