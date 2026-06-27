namespace CMG.Runner;

public static class CmgShard
{
    public static bool TryParse(string? value, out int index, out int count, out string? error)
    {
        index = 1;
        count = 1;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var parts = value.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length is not 2 ||
            !int.TryParse(parts[0], out index) ||
            !int.TryParse(parts[1], out count) ||
            index < 1 ||
            count < 1 ||
            index > count)
        {
            error = "--shard must use index/count with 1 <= index <= count.";
            return false;
        }

        return true;
    }
}
