using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgDslParserTests
{
    [Fact]
    public void Parse_ReadsSuiteTestAndSteps()
    {
        var parser = new CmgDslParser();
        var result = parser.Parse("flow.cmgscript", """
        suite "Visual flow" {
          test "opens dialog" {
            step "Open" {
              click "#open"
            }
          }
        }
        """);

        Assert.True(result.Success);
        var suite = Assert.Single(result.Document!.Nodes);
        Assert.Equal("suite", suite.Kind);
        Assert.Equal("Visual flow", suite.Name);
        Assert.Equal("test", Assert.Single(suite.Children).Kind);
    }

    [Fact]
    public void Parse_NormalizesProviderDeclarationAliases()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        describe.skip "legacy" {
          test.fixme "broken"
          it.todo "queued"
        }
        test.only "focused" {
          caption "run"
        }
        """);

        Assert.True(result.Success, result.Error);
        var suite = result.Document!.Nodes[0];
        Assert.Equal("describe", suite.Kind);
        Assert.Equal("true", suite.Options["skip"]);
        Assert.Equal("test", suite.Children[0].Kind);
        Assert.Equal("true", suite.Children[0].Options["skip"]);
        Assert.Contains("fixme", suite.Children[0].Options["reason"]);
        Assert.Equal("it", suite.Children[1].Kind);
        Assert.Contains("todo", suite.Children[1].Options["reason"]);
        Assert.Equal("test", result.Document.Nodes[1].Kind);
        Assert.Equal("true", result.Document.Nodes[1].Options["only"]);
    }

    [Fact]
    public void Parse_ReportsMissingBlockClose()
    {
        var parser = new CmgDslParser();
        var result = parser.Parse("bad.cmgscript", """
        test "broken" {
          click "#open"
        """);

        Assert.False(result.Success);
        Assert.Contains("missing block close", result.Error);
    }

    [Fact]
    public void Parse_PreservesOptionsAndEscapes()
    {
        var parser = new CmgDslParser();
        var result = parser.Parse("flow.cmgscript", """
        test "typing" {
          type "#name" "Line\nTwo" timeout=5000 header.Authorization="Bearer token"
        }
        """);

        var action = Assert.Single(Assert.Single(result.Document!.Nodes).Children);
        Assert.Equal("Line\nTwo", action.Arguments[1]);
        Assert.Equal("5000", action.Options["timeout"]);
        Assert.Equal("Bearer token", action.Options["header.Authorization"]);
    }

    [Fact]
    public void Parse_PreservesEmptyQuotedStrings()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        test "empty" {
          if "" == "" {
            evaluate "true"
          }
        }
        """);

        Assert.True(result.Success);
        var action = Assert.Single(Assert.Single(result.Document!.Nodes).Children);
        Assert.Equal("", action.Arguments[0]);
        Assert.Equal("", action.Arguments[2]);
    }

    [Fact]
    public void Parse_RejectsFlatV1Scripts()
    {
        var result = new CmgDslParser().Parse("old.cmgscript", """
        navigate "https://example.com"
        click "#open"
        """);

        Assert.False(result.Success);
        Assert.Contains("test/it/specify or suite/describe/context", result.Error);
    }

    [Fact]
    public void Parse_ImportsMacrosRelativeToSourceFile()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "shared.cmg"), """
            macro helper target {
              click "${target}"
            }
            """);

            var result = new CmgDslParser().Parse(Path.Combine(directory.FullName, "flow.cmg"), """
            import "shared.cmg"
            test "uses import" {
              call helper "#run"
            }
            """);

            Assert.True(result.Success, result.Error);
            Assert.Equal("macro", result.Document!.Nodes[0].Kind);
            Assert.Equal("test", result.Document.Nodes[1].Kind);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Parse_AllowsOddBranchSpacingAndImportCasing()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "shared.cmgscript"), """
            macro note {
              caption "imported"
            }
            """);

            var result = new CmgDslParser().Parse(Path.Combine(directory.FullName, "flow.cmgscript"), """
              IMPORT    "shared.cmgscript"

              test   "spacing"   {
                if false {
                  caption "if"
                }ELSE{
                  call note
                }
                try {
                  caption "try"
                }   Catch   error   {
                  caption "${error}"
                }FINALLY{
                  caption "done"
                }
              }
            """);

            Assert.True(result.Success, result.Error);
            var test = result.Document!.Nodes.Single(node => node.Kind.Equals("test", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(["if", "else", "try", "catch", "finally"], test.Children.Select(action => action.Kind.ToLowerInvariant()));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Parse_AllowsInlineBlocksAcrossDsl()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
        describe "inline" { before { setContent "<main>{ready}</main>" } it "case" { if true { caption "yes" } else { caption "no" } } after { caption "done" } }
        """);

        Assert.True(result.Success, result.Error);
        var suite = Assert.Single(result.Document!.Nodes);
        Assert.Equal("describe", suite.Kind);
        Assert.Equal(["before", "it", "after"], suite.Children.Select(node => node.Kind.ToLowerInvariant()));
        Assert.Equal("<main>{ready}</main>", suite.Children[0].Children[0].Arguments[0]);
    }

    [Fact]
    public void Parse_AllowsHeavyIndentationTabsAndRepeatedSpacing()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", "          test          \"spacing\"          tag=smoke          {\r\n\t\tclick          \"#save\"          timeout=5000\r\n          }\r\n");

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(result.Document!.Nodes);
        Assert.Equal("test", test.Kind);
        Assert.Equal("spacing", test.Name);
        Assert.Equal("smoke", test.Options["tag"]);
        var action = Assert.Single(test.Children);
        Assert.Equal("click", action.Kind);
        Assert.Equal("#save", action.Arguments[0]);
        Assert.Equal("5000", action.Options["timeout"]);
    }

    [Fact]
    public void Parse_AllowsMessySpacingInsideNestedRunnerBlocks()
    {
        var result = new CmgDslParser().Parse("flow.cmgscript", """
                    describe          "spacing"          {
                              it          "runs"          {
                                        if          true          {
                                                  click          "#save"          timeout=5000
                                        }          else          {
                                                  caption          "not ready"
                                        }
                              }
                    }
        """);

        Assert.True(result.Success, result.Error);
        var test = Assert.Single(Assert.Single(result.Document!.Nodes).Children);
        Assert.Equal("it", test.Kind);
        Assert.Equal(["if", "else"], test.Children.Select(node => node.Kind.ToLowerInvariant()));
        Assert.Equal("#save", test.Children[0].Children[0].Arguments[0]);
    }
}
