using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteApiRequest(BrowserScriptAction action) =>
        ExecuteRunnerStep(action, new CmgApiRequestRunner().Run(ToNode(action)));

    private static IReadOnlyList<string> ExecuteStorageState(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action) =>
        ExecuteRunnerStep(action, new CmgStorageStateRunner().Run(ToNode(action), remoteDebuggingUrl, automationClient));

    private static IReadOnlyList<string> ExecuteVisualAssertion(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action) =>
        ExecuteRunnerStep(action, new CmgVisualAssertionRunner().Run(ToNode(action), remoteDebuggingUrl, automationClient));

    private static IReadOnlyList<string> ExecuteUploadFiles(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action) =>
        ExecuteRunnerStep(action, new CmgUploadRunner().Run(ToNode(action), remoteDebuggingUrl, automationClient));

    private static IReadOnlyList<string> ExecuteRunnerStep(BrowserScriptAction action, CmgStepResult step)
    {
        if (step.Success)
        {
            return step.Output;
        }

        throw new ScriptExecutionException(step.Error ?? $"{action.Name} failed.");
    }

    private static CmgNode ToNode(BrowserScriptAction action) =>
        new(
            action.LineNumber,
            action.Name,
            action.Arguments.FirstOrDefault() ?? action.Name,
            action.Arguments,
            action.Options,
            []);
}
