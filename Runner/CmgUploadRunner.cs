using CMG.Browser;

namespace CMG.Runner;

public sealed class CmgUploadRunner
{
    public CmgStepResult Run(CmgNode action, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        if (action.Arguments.Count < 2)
        {
            return Fail(action, "uploadFiles requires a selector and at least one file path.");
        }

        var selector = CmgLocator.ToCssSelector(action.Arguments[0]);
        var files = action.Arguments.Skip(1).Select(Path.GetFullPath).ToArray();
        var missing = files.FirstOrDefault(file => !File.Exists(file));
        if (missing is not null)
        {
            return Fail(action, $"Upload file '{missing}' was not found.");
        }

        try
        {
            automationClient.Evaluate(remoteDebuggingUrl, BuildScript(selector, files));
            return new CmgStepResult(action.LineNumber, action.Kind, true, [$"UPLOAD {action.LineNumber:000} {files.Length}"], null, null);
        }
        catch (Exception exception) when (exception is IOException or ChromeDevToolsException)
        {
            return Fail(action, exception.Message);
        }
    }

    private static string BuildScript(string selector, IReadOnlyList<string> files)
    {
        var entries = string.Join(",", files.Select(file => $"{{ name: {QuoteJs(Path.GetFileName(file))}, data: {QuoteJs(Convert.ToBase64String(File.ReadAllBytes(file)))} }}"));
        return $$"""
        (() => {
          const selector = {{QuoteJs(selector)}};
          const input = document.querySelector(selector);
          if (!input) throw new Error(`No element matched selector ${selector}`);
          const transfer = new DataTransfer();
          for (const file of [{{entries}}]) {
            const binary = atob(file.data);
            const bytes = new Uint8Array(binary.length);
            for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
            transfer.items.add(new File([bytes], file.name));
          }
          input.files = transfer.files;
          input.dispatchEvent(new Event('input', { bubbles: true }));
          input.dispatchEvent(new Event('change', { bubbles: true }));
          return true;
        })()
        """;
    }

    private static CmgStepResult Fail(CmgNode action, string error) => new(action.LineNumber, action.Kind, false, [], error, null);

    private static string QuoteJs(string value) =>
        $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";
}
