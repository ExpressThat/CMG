namespace CMG.Runner;

public static class CmgAnnotations
{
    public static IReadOnlyList<CmgAnnotation> FromOptions(IReadOnlyDictionary<string, string> options)
    {
        var annotations = new List<CmgAnnotation>();
        AddKnown(annotations, options, "owner");
        AddKnown(annotations, options, "issue");
        AddKnown(annotations, options, "link");
        AddKnown(annotations, options, "requirement");
        AddKnown(annotations, options, "note");
        annotations.AddRange(options
            .Where(option => option.Key.StartsWith("annotation.", StringComparison.OrdinalIgnoreCase))
            .Select(option => new CmgAnnotation(option.Key["annotation.".Length..], option.Value))
            .Where(annotation => !string.IsNullOrWhiteSpace(annotation.Type)));
        return annotations;
    }

    private static void AddKnown(
        List<CmgAnnotation> annotations,
        IReadOnlyDictionary<string, string> options,
        string name)
    {
        if (options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            annotations.Add(new CmgAnnotation(name, value));
        }
    }
}
