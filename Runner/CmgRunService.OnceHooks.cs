namespace CMG.Runner;

public sealed partial class CmgRunService
{
    internal static IReadOnlyList<CmgTestCase> ApplyOnceHooks(IReadOnlyList<CmgTestCase> tests)
    {
        var runnable = tests
            .Select((test, index) => new IndexedTest(index, test))
            .Where(item => !IsSkipped(item.Test))
            .ToArray();
        if (runnable.Length is 0)
        {
            return tests;
        }

        var result = tests.ToArray();
        DecorateScope(
            result,
            runnable.Where(item => item.Test.SuiteName is not null).ToArray(),
            key: item => $"{item.Test.SourcePath}\n{item.Test.SuiteName}",
            before: item => item.Test.SuiteBeforeAll,
            after: item => item.Test.SuiteAfterAll);
        DecorateScope(
            result,
            runnable,
            key: item => item.Test.SourcePath,
            before: item => item.Test.RootBeforeAll,
            after: item => item.Test.RootAfterAll);
        return result;
    }

    private static void DecorateScope(
        CmgTestCase[] tests,
        IReadOnlyList<IndexedTest> runnable,
        Func<IndexedTest, string> key,
        Func<IndexedTest, IReadOnlyList<CmgNode>> before,
        Func<IndexedTest, IReadOnlyList<CmgNode>> after)
    {
        foreach (var group in runnable.GroupBy(key))
        {
            var first = group.First();
            var last = group.Last();
            AddBeforeHooks(tests, first.Index, before(first));
            AddAfterHooks(tests, last.Index, after(last));
        }
    }

    private static void AddBeforeHooks(CmgTestCase[] tests, int index, IReadOnlyList<CmgNode> hooks)
    {
        if (hooks.Count is 0) return;
        var prefix = tests[index].Actions.TakeWhile(action => action.Kind.Equals("macro", StringComparison.OrdinalIgnoreCase)).Count();
        tests[index] = tests[index] with { Actions = [.. tests[index].Actions.Take(prefix), .. hooks, .. tests[index].Actions.Skip(prefix)] };
    }

    private static void AddAfterHooks(CmgTestCase[] tests, int index, IReadOnlyList<CmgNode> hooks)
    {
        if (hooks.Count is 0) return;
        tests[index] = tests[index] with { Actions = [.. tests[index].Actions, .. hooks] };
    }

    private sealed record IndexedTest(int Index, CmgTestCase Test);
}
