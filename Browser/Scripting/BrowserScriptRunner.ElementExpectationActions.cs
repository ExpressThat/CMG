using CMG.Browser;
using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteElementExpectation(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        var mode = action.Name.ToLowerInvariant() switch
        {
            "expectvisible" => "visible",
            "expecthidden" => "hidden",
            "expectenabled" => "enabled",
            "expectdisabled" => "disabled",
            "expectvalue" => "value",
            "expectattribute" => "attribute",
            "expectchecked" => "checked",
            "expectcount" => "count",
            _ => throw new ScriptExecutionException($"Unknown element expectation '{action.Name}'.")
        };
        action = NormalizeLocatorArgument(action);
        var plan = CmgExpectationScripts.Expressions(ToNode(action), mode);
        if (plan.Error is not null)
        {
            throw new ScriptExecutionException(plan.Error);
        }

        RunExpectationUntilReady(remoteDebuggingUrl, automationClient, action, plan);
        return [$"EXPECT {action.LineNumber:000} {mode} {action.Arguments[0]}"];
    }

    private static void RunExpectationUntilReady(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        CmgExpectationPlan plan)
    {
        var timeout = GetIntOption(action, "timeout", 0);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        Exception? last = null;
        do
        {
            try
            {
                foreach (var expression in plan.PrefixExpressions)
                {
                    automationClient.Evaluate(remoteDebuggingUrl, expression);
                }

                automationClient.Evaluate(remoteDebuggingUrl, plan.Expression);
                return;
            }
            catch (ChromeDevToolsException exception)
            {
                last = exception;
                if (timeout <= 0)
                {
                    throw;
                }

                Thread.Sleep(50);
            }
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException(last?.Message ?? $"{action.Name} did not pass within {timeout}ms.");
    }

    private static BrowserScriptAction NormalizeLocatorArgument(BrowserScriptAction action)
    {
        var locator = action.Options.FirstOrDefault(pair => IsLocatorOption(pair.Key));
        if (string.IsNullOrWhiteSpace(locator.Key))
        {
            return action;
        }

        return action with { Arguments = [$"{locator.Key}={locator.Value}", .. action.Arguments] };
    }

}
