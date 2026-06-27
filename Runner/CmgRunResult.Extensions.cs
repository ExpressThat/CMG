namespace CMG.Runner;

public sealed partial record CmgRunResult
{
    public static CmgRunResult Fail(string error) => new(false, [], [], error);
}
