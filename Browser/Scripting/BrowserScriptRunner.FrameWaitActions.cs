using System.Text.Json;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteFrameWait(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeFrameSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);
        WaitForFrameSelector(remoteDebuggingUrl, automationClient, action.Arguments[0], action.Arguments[1], GetIntOption(action, "timeout", 5_000));
        return [$"FRAME {action.LineNumber:000} {action.Name}"];
    }

    private static void WaitForFrameSelector(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string frameSelector,
        string selector,
        int timeout)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        var snapshot = new FrameSelectorSnapshot(false, false);
        do
        {
            snapshot = ReadFrameSelectorState(automationClient.Evaluate(remoteDebuggingUrl, BrowserFrameScripts.SelectorState(frameSelector, selector)));
            if (snapshot.Attached)
            {
                return;
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException(
            $"Frame selector {selector} in {frameSelector} did not appear within {timeout}ms. Last state: {snapshot}.");
    }

    private static FrameSelectorSnapshot ReadFrameSelectorState(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return new FrameSelectorSnapshot(
            root.TryGetProperty("attached", out var attached) && attached.GetBoolean(),
            root.TryGetProperty("visible", out var visible) && visible.GetBoolean());
    }

    private sealed record FrameSelectorSnapshot(bool Attached, bool Visible)
    {
        public override string ToString() => $"attached={Attached.ToString().ToLowerInvariant()}, visible={Visible.ToString().ToLowerInvariant()}";
    }
}
