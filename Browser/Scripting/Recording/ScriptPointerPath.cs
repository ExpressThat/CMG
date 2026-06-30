namespace CMG.Browser.Scripting.Recording;

public enum ScriptPointerPath
{
    Direct,
    Arc,
    Manhattan,
    AvoidTarget,
    AvoidCenter
}

public static class ScriptPointerPathParser
{
    public const string Values = "direct, arc, manhattan, avoid-target, avoid-center";

    public static bool TryParse(string value, out ScriptPointerPath path)
    {
        path = value.Trim().ToLowerInvariant() switch
        {
            "direct" or "line" => ScriptPointerPath.Direct,
            "arc" or "curved" or "curve" => ScriptPointerPath.Arc,
            "manhattan" or "orthogonal" => ScriptPointerPath.Manhattan,
            "avoid-target" or "avoidtarget" => ScriptPointerPath.AvoidTarget,
            "avoid-center" or "avoidcenter" => ScriptPointerPath.AvoidCenter,
            _ => (ScriptPointerPath)(-1)
        };

        return Enum.IsDefined(path);
    }
}
