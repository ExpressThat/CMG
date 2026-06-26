namespace CMG.Runner;

public sealed partial class CmgTestPlanner
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
                foreach (var test in ExpandParameterizedTests(node))
                {
                    tests.Add(BuildTest(document.SourcePath, test, rootMacros, rootBeforeEach, rootAfterEach, null) with
                    {
                        RootBeforeAll = rootBeforeAll,
                        RootAfterAll = rootAfterAll
                    });
                }
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

        foreach (var child in suite.Children.Where(IsTest).SelectMany(ExpandParameterizedTests))
        {
            tests.Add(BuildTest(document.SourcePath, child, macros, beforeEach, afterEach, suite.Name, suite.Options) with
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
        string? suiteName,
        IReadOnlyDictionary<string, string>? suiteOptions = null)
    {
        var name = suiteName is null ? test.Name : $"{suiteName} / {test.Name}";
        var options = MergeOptions(suiteOptions, test.Options);
        var declarationVariables = CmgVariables.FromDeclarationOptions(test.LineNumber, options);
        var actions = declarationVariables.Concat(macros).Concat(beforeEach).Concat(test.Children).Concat(afterEach).ToArray();
        return new CmgTestCase(sourcePath, name, actions, options)
        {
            SuiteName = suiteName,
            Annotations = BuildAnnotations(options)
        };
    }

    private static IReadOnlyDictionary<string, string> MergeOptions(
        IReadOnlyDictionary<string, string>? suiteOptions,
        IReadOnlyDictionary<string, string> testOptions)
    {
        if (suiteOptions is null || suiteOptions.Count is 0)
        {
            return testOptions;
        }

        var merged = new Dictionary<string, string>(suiteOptions, StringComparer.OrdinalIgnoreCase);
        foreach (var option in testOptions)
        {
            merged[option.Key] = option.Value;
        }

        if (suiteOptions.TryGetValue("only", out var only) && IsTruthy(only))
        {
            merged["only"] = "true";
        }

        if (suiteOptions.TryGetValue("skip", out var skip) && IsTruthy(skip))
        {
            merged["skip"] = "true";
            if (suiteOptions.TryGetValue("reason", out var reason))
            {
                merged.TryAdd("reason", reason);
            }
        }

        if (suiteOptions.TryGetValue("slow", out var slow) && IsEnabledOption(slow) && !testOptions.ContainsKey("slow"))
        {
            merged["slow"] = slow;
        }

        return merged;
    }

    private static IReadOnlyList<CmgAnnotation> BuildAnnotations(IReadOnlyDictionary<string, string> options) =>
        CmgAnnotations.FromOptions(options);

    private static bool IsSuite(CmgNode node) =>
        IsAny(node, "suite", "describe", "context");

    private static bool IsTest(CmgNode node) =>
        IsAny(node, "test", "it", "specify");

    private static bool IsMacro(CmgNode node) => node.Kind.Equals("macro", StringComparison.OrdinalIgnoreCase);

    private static bool IsBeforeAll(CmgNode node) =>
        IsAny(node, "beforeAll", "before");

    private static bool IsAfterAll(CmgNode node) =>
        IsAny(node, "afterAll", "after");

    private static bool IsBeforeEach(CmgNode node) => node.Kind.Equals("beforeEach", StringComparison.OrdinalIgnoreCase);

    private static bool IsAfterEach(CmgNode node) => node.Kind.Equals("afterEach", StringComparison.OrdinalIgnoreCase);

    private static bool IsAny(CmgNode node, params string[] names) =>
        names.Any(name => node.Kind.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static bool IsTruthy(string value) =>
        value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("yes", StringComparison.OrdinalIgnoreCase);

    private static bool IsEnabledOption(string value) =>
        IsTruthy(value) ||
        (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) && number > 0);
}
