using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CMG.Tests;

public sealed class ReleaseSkillPackagingTests
{
    [Fact]
    public void ReleaseWorkflow_PackagesReferencesWithoutInliningThem()
    {
        var root = FindRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "release.yml"));

        Assert.Contains("./skill/Build-SkillPackage.ps1", workflow);
        Assert.Contains("RELEASE_DIR }}\\agents", workflow);
        Assert.Contains("RELEASE_DIR }}\\references", workflow);
        Assert.DoesNotContain("Add-Content -Path $skillPath", workflow);
        Assert.DoesNotContain("$contentFiles", workflow);
    }

    [Fact]
    public void BuildSkillPackage_ProducesCompactRouterAndOnDemandReferences()
    {
        var root = FindRepositoryRoot();
        var output = Path.Combine(Path.GetTempPath(), $"cmg-skill-{Guid.NewGuid():N}");

        try
        {
            var start = new ProcessStartInfo("pwsh")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add("-NoProfile");
            start.ArgumentList.Add("-File");
            start.ArgumentList.Add(Path.Combine(root, "skill", "Build-SkillPackage.ps1"));
            start.ArgumentList.Add("-ReleaseDirectory");
            start.ArgumentList.Add(output);

            using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start PowerShell.");
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.True(process.ExitCode is 0, $"{stdout}{Environment.NewLine}{stderr}");
            var skillPath = Path.Combine(output, "SKILL.md");
            var skill = File.ReadAllText(skillPath);
            Assert.True(new FileInfo(skillPath).Length <= 65_536);
            Assert.True(File.ReadLines(skillPath).Count() <= 400);
            Assert.DoesNotContain("## Source:", skill);
            Assert.Contains("references/docs/commands.md", skill);
            Assert.True(File.Exists(Path.Combine(output, "agents", "openai.yaml")));
            Assert.True(File.Exists(Path.Combine(output, "references", "README.md")));
            Assert.True(File.Exists(Path.Combine(output, "references", "docs", "commands.md")));
            Assert.True(File.Exists(Path.Combine(output, "references", "demo-scripts", "README.md")));
            Assert.True(File.Exists(Path.Combine(output, "references", "demo-scripts", "159-controlled-input-remount.cmgscript")));
            Assert.True(Directory.GetFiles(Path.Combine(output, "references", "docs", "scripting", "action-topics"), "*.md").Length >= 100);
            var commandDirectory = Path.Combine(output, "references", "docs", "scripting", "commands");
            Assert.True(Directory.GetFiles(commandDirectory, "*.md").Length >= 200);
            Assert.True(File.Exists(Path.Combine(commandDirectory, "fill.md")));
            Assert.True(File.Exists(Path.Combine(commandDirectory, "waitforelement.md")));
            Assert.True(File.Exists(Path.Combine(commandDirectory, "recordvideo.md")));
            AssertAllMarkdownIsReachable(output);
        }
        finally
        {
            if (Directory.Exists(output))
            {
                Directory.Delete(output, recursive: true);
            }
        }
    }

    private static void AssertAllMarkdownIsReachable(string root)
    {
        var pending = new Queue<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        pending.Enqueue(Path.Combine(root, "SKILL.md"));

        while (pending.TryDequeue(out var current))
        {
            current = Path.GetFullPath(current);
            if (!visited.Add(current))
            {
                continue;
            }

            foreach (Match match in Regex.Matches(File.ReadAllText(current), @"\[[^\]]+\]\(([^)#]+\.md)(?:#[^)]*)?\)"))
            {
                var target = Uri.UnescapeDataString(match.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar));
                if (Uri.TryCreate(target, UriKind.Absolute, out _))
                {
                    continue;
                }

                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(current)!, target));
                if (resolved.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.True(File.Exists(resolved), $"Broken Markdown link in {Path.GetRelativePath(root, current)}: {match.Groups[1].Value}");
                    pending.Enqueue(resolved);
                }
            }
        }

        var markdown = Directory.GetFiles(root, "*.md", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .ToArray();
        var unreachable = markdown.Where(path => !visited.Contains(path)).Select(path => Path.GetRelativePath(root, path));
        Assert.True(markdown.All(visited.Contains), $"Unreachable Markdown:{Environment.NewLine}{string.Join(Environment.NewLine, unreachable)}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CMG.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
