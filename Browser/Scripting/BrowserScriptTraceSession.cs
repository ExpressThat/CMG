namespace CMG.Browser.Scripting;

internal sealed class BrowserScriptTraceSession
{
    private readonly List<BrowserScriptTraceStep> steps = [];

    public BrowserScriptTraceSession(string? outputPath, bool suppressNested = false)
    {
        OutputPath = string.IsNullOrWhiteSpace(outputPath) ? null : Path.GetFullPath(outputPath);
        SuppressNested = suppressNested;
    }

    public string? OutputPath { get; private set; }

    public bool SuppressNested { get; }

    public IReadOnlyList<BrowserScriptTraceStep> Steps => steps;

    public bool IsActive { get; private set; } = true;

    public void Record(BrowserScriptAction action, bool success, string? error, IReadOnlyList<string> output)
    {
        if (!IsActive)
        {
            return;
        }

        steps.Add(new BrowserScriptTraceStep(action.LineNumber, action.Name, success, error, output));
    }

    public string Finish(string? path, bool success, string? error)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            OutputPath = Path.GetFullPath(path);
        }

        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            throw new ScriptExecutionException("stopTracing requires path= or output= when startTracing did not set one.");
        }

        BrowserScriptTraceWriter.Write(OutputPath, success, error, steps);
        IsActive = false;
        return OutputPath;
    }
}
