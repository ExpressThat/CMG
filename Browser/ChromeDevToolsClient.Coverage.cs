using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public void StartCoverage(string remoteDebuggingUrl, CoverageOptions options)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            if (options.JavaScript)
            {
                await session.SendCommand("Profiler.enable");
                await session.SendCommand("Profiler.startPreciseCoverage", writer =>
                {
                    writer.WriteBoolean("callCount", true);
                    writer.WriteBoolean("detailed", true);
                });
            }

            if (options.Css)
            {
                await session.SendCommand("DOM.enable");
                await session.SendCommand("CSS.enable");
                await session.SendCommand("CSS.startRuleUsageTracking");
            }

            return true;
        });
    }

    public string StopCoverage(string remoteDebuggingUrl)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            var js = await TakeJsCoverage(session);
            var css = await TakeCssCoverage(session);
            return $$"""{"js":{{js}},"css":{{css}}}""";
        });
    }

    private static async Task<string> TakeJsCoverage(DevToolsSession session)
    {
        try
        {
            var response = await session.SendCommand("Profiler.takePreciseCoverage");
            await session.SendCommand("Profiler.stopPreciseCoverage");
            return response.GetProperty("result").GetProperty("result").GetRawText();
        }
        catch (ChromeDevToolsException)
        {
            return "[]";
        }
    }

    private static async Task<string> TakeCssCoverage(DevToolsSession session)
    {
        try
        {
            var response = await session.SendCommand("CSS.stopRuleUsageTracking");
            return response.GetProperty("result").GetProperty("ruleUsage").GetRawText();
        }
        catch (ChromeDevToolsException)
        {
            return "[]";
        }
    }
}
