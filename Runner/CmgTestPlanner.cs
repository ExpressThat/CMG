namespace CMG.Runner;

public sealed class CmgTestPlanner
{
    public IReadOnlyList<CmgTestCase> Plan(CmgDocument document)
    {
        var rootBeforeEach = document.Nodes.Where(IsBeforeEach).SelectMany(node => node.Children).ToArray();
        var rootAfterEach = document.Nodes.Where(IsAfterEach).SelectMany(node => node.Children).ToArray();
        var tests = new List<CmgTestCase>();

        foreach (var node in document.Nodes)
        {
            if (IsTest(node))
            {
                tests.Add(BuildTest(document.SourcePath, node, rootBeforeEach, rootAfterEach, suiteName: null));
            }
            else if (IsSuite(node))
            {
                AddSuiteTests(document, node, rootBeforeEach, rootAfterEach, tests);
            }
        }

        return tests;
    }

    private static void AddSuiteTests(
        CmgDocument document,
        CmgNode suite,
        IReadOnlyList<CmgNode> rootBeforeEach,
        IReadOnlyList<CmgNode> rootAfterEach,
        List<CmgTestCase> tests)
    {
        var suiteBeforeEach = suite.Children.Where(IsBeforeEach).SelectMany(node => node.Children).ToArray();
        var suiteAfterEach = suite.Children.Where(IsAfterEach).SelectMany(node => node.Children).ToArray();
        var beforeEach = rootBeforeEach.Concat(suiteBeforeEach).ToArray();
        var afterEach = suiteAfterEach.Concat(rootAfterEach).ToArray();

        foreach (var child in suite.Children.Where(IsTest))
        {
            tests.Add(BuildTest(document.SourcePath, child, beforeEach, afterEach, suite.Name));
        }
    }

    private static CmgTestCase BuildTest(
        string sourcePath,
        CmgNode test,
        IReadOnlyList<CmgNode> beforeEach,
        IReadOnlyList<CmgNode> afterEach,
        string? suiteName)
    {
        var name = suiteName is null ? test.Name : $"{suiteName} / {test.Name}";
        var actions = beforeEach.Concat(test.Children).Concat(afterEach).ToArray();
        return new CmgTestCase(sourcePath, name, actions, test.Options);
    }

    private static bool IsSuite(CmgNode node) => node.Kind.Equals("suite", StringComparison.OrdinalIgnoreCase);

    private static bool IsTest(CmgNode node) => node.Kind.Equals("test", StringComparison.OrdinalIgnoreCase);

    private static bool IsBeforeEach(CmgNode node) => node.Kind.Equals("beforeEach", StringComparison.OrdinalIgnoreCase);

    private static bool IsAfterEach(CmgNode node) => node.Kind.Equals("afterEach", StringComparison.OrdinalIgnoreCase);
}
