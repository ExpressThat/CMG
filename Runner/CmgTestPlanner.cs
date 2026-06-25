namespace CMG.Runner;

public sealed class CmgTestPlanner
{
    public IReadOnlyList<CmgTestCase> Plan(CmgDocument document)
    {
        var rootMacros = document.Nodes.Where(IsMacro).ToArray();
        var rootBeforeAll = document.Nodes.Where(IsBeforeAll).SelectMany(node => node.Children).ToArray();
        var rootAfterAll = document.Nodes.Where(IsAfterAll).SelectMany(node => node.Children).ToArray();
        var rootBeforeEach = document.Nodes.Where(IsBeforeEach).SelectMany(node => node.Children).ToArray();
        var rootAfterEach = document.Nodes.Where(IsAfterEach).SelectMany(node => node.Children).ToArray();
        var tests = new List<CmgTestCase>();

        foreach (var node in document.Nodes)
        {
            if (IsTest(node))
            {
                tests.Add(BuildTest(document.SourcePath, node, rootMacros, rootBeforeEach, rootAfterEach, null) with
                {
                    RootBeforeAll = rootBeforeAll,
                    RootAfterAll = rootAfterAll
                });
            }
            else if (IsSuite(node))
            {
                AddSuiteTests(document, node, rootMacros, rootBeforeAll, rootAfterAll, rootBeforeEach, rootAfterEach, tests);
            }
        }

        return tests;
    }

    private static void AddSuiteTests(
        CmgDocument document,
        CmgNode suite,
        IReadOnlyList<CmgNode> rootMacros,
        IReadOnlyList<CmgNode> rootBeforeAll,
        IReadOnlyList<CmgNode> rootAfterAll,
        IReadOnlyList<CmgNode> rootBeforeEach,
        IReadOnlyList<CmgNode> rootAfterEach,
        List<CmgTestCase> tests)
    {
        var suiteMacros = suite.Children.Where(IsMacro).ToArray();
        var suiteBeforeAll = suite.Children.Where(IsBeforeAll).SelectMany(node => node.Children).ToArray();
        var suiteAfterAll = suite.Children.Where(IsAfterAll).SelectMany(node => node.Children).ToArray();
        var suiteBeforeEach = suite.Children.Where(IsBeforeEach).SelectMany(node => node.Children).ToArray();
        var suiteAfterEach = suite.Children.Where(IsAfterEach).SelectMany(node => node.Children).ToArray();
        var macros = rootMacros.Concat(suiteMacros).ToArray();
        var beforeEach = rootBeforeEach.Concat(suiteBeforeEach).ToArray();
        var afterEach = suiteAfterEach.Concat(rootAfterEach).ToArray();

        foreach (var child in suite.Children.Where(IsTest))
        {
            tests.Add(BuildTest(document.SourcePath, child, macros, beforeEach, afterEach, suite.Name) with
            {
                RootBeforeAll = rootBeforeAll,
                RootAfterAll = rootAfterAll,
                SuiteBeforeAll = suiteBeforeAll,
                SuiteAfterAll = suiteAfterAll
            });
        }
    }

    private static CmgTestCase BuildTest(
        string sourcePath,
        CmgNode test,
        IReadOnlyList<CmgNode> macros,
        IReadOnlyList<CmgNode> beforeEach,
        IReadOnlyList<CmgNode> afterEach,
        string? suiteName)
    {
        var name = suiteName is null ? test.Name : $"{suiteName} / {test.Name}";
        var actions = macros.Concat(beforeEach).Concat(test.Children).Concat(afterEach).ToArray();
        return new CmgTestCase(sourcePath, name, actions, test.Options) { SuiteName = suiteName };
    }

    private static bool IsSuite(CmgNode node) => node.Kind.Equals("suite", StringComparison.OrdinalIgnoreCase);

    private static bool IsTest(CmgNode node) => node.Kind.Equals("test", StringComparison.OrdinalIgnoreCase);

    private static bool IsMacro(CmgNode node) => node.Kind.Equals("macro", StringComparison.OrdinalIgnoreCase);

    private static bool IsBeforeAll(CmgNode node) => node.Kind.Equals("beforeAll", StringComparison.OrdinalIgnoreCase);

    private static bool IsAfterAll(CmgNode node) => node.Kind.Equals("afterAll", StringComparison.OrdinalIgnoreCase);

    private static bool IsBeforeEach(CmgNode node) => node.Kind.Equals("beforeEach", StringComparison.OrdinalIgnoreCase);

    private static bool IsAfterEach(CmgNode node) => node.Kind.Equals("afterEach", StringComparison.OrdinalIgnoreCase);
}
