namespace CMG.Runner;

public sealed partial class CmgRunService
{
    internal static IReadOnlyList<CmgTestCase> SelectFocusedTests(IReadOnlyList<CmgTestCase> tests) =>
        tests.Any(IsOnly) ? tests.Where(IsOnly).ToArray() : tests;

    internal static IReadOnlyList<CmgTestCase> RepeatTests(IReadOnlyList<CmgTestCase> tests, int repeatEach)
    {
        if (repeatEach <= 1)
        {
            return tests;
        }

        var repeated = new List<CmgTestCase>(tests.Count * repeatEach);
        foreach (var test in tests)
        {
            for (var index = 1; index <= repeatEach; index++)
            {
                repeated.Add(test with { Name = $"{test.Name} [repeat {index}/{repeatEach}]" });
            }
        }

        return repeated;
    }

    private static bool IsOnly(CmgTestCase test) => IsTruthyOption(test, "only");
}
