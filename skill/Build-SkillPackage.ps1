param(
    [Parameter(Mandatory = $true)]
    [string]$ReleaseDirectory
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$releaseRoot = [System.IO.Path]::GetFullPath($ReleaseDirectory)
$referenceRoot = Join-Path $releaseRoot "references"

function ConvertTo-Slug([string]$Value) {
    $plain = $Value.Replace('`', '').ToLowerInvariant()
    return (($plain -replace '[^a-z0-9]+', '-').Trim('-'))
}

function Export-ActionTopics([string]$Source, [string]$Target, [string]$DispatchSource) {
    New-Item -ItemType Directory -Force -Path $Target | Out-Null
    $lines = Get-Content -LiteralPath $Source
    $topics = @()
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -notmatch '^(#{2,3})\s+(.+)$') { continue }
        $level = $Matches[1].Length
        $title = $Matches[2]
        $end = $index + 1
        while ($end -lt $lines.Count) {
            if ($lines[$end] -match '^(#+)\s+' -and $Matches[1].Length -le $level) { break }
            $end++
        }

        $slug = ConvertTo-Slug $title
        $baseSlug = $slug
        $suffix = 2
        while (Test-Path (Join-Path $Target "$slug.md")) {
            $slug = "$baseSlug-$suffix"
            $suffix++
        }

        $body = if ($end -gt $index + 1) { $lines[($index + 1)..($end - 1)] } else { @() }
        @("# $($title.Replace('`', ''))", "", "Source: [complete action reference](../actions.md).", "") + $body |
            Set-Content -LiteralPath (Join-Path $Target "$slug.md") -Encoding utf8NoBOM
        $topics += [pscustomobject]@{ Title = $title.Replace('`', ''); File = "$slug.md" }
    }

    @("# Script Action Topics", "", "Open one behavior-family leaf instead of loading the complete action reference.", "") +
        ($topics | ForEach-Object { "- [$($_.Title)]($($_.File))" }) |
        Set-Content -LiteralPath (Join-Path $Target "index.md") -Encoding utf8NoBOM

    $commandsTarget = Join-Path (Split-Path -Parent $Target) "commands"
    New-Item -ItemType Directory -Force -Path $commandsTarget | Out-Null
    $dispatch = Get-Content -LiteralPath $DispatchSource -Raw
    $commands = [regex]::Matches($dispatch, '"([A-Za-z][A-Za-z0-9.]*)"') |
        ForEach-Object { $_.Groups[1].Value } |
        Sort-Object -Unique
    $commandLinks = @()
    foreach ($command in $commands) {
        $commandSlug = ConvertTo-Slug $command
        $topic = $topics | Where-Object { $_.Title -match "(?i)(^|[^a-z0-9])$([regex]::Escape($command))([^a-z0-9]|$)" } | Select-Object -First 1
        $topicLink = if ($null -eq $topic) { "../action-topics/index.md" } else { "../action-topics/$($topic.File)" }
        @(
            "# ``$command``",
            "",
            "Read the [shared behavior reference]($topicLink) for syntax, options, output, GIF behavior, and failures.",
            "",
            "For additional context, search for ``$command`` in the [complete action reference](../actions.md)."
        ) | Set-Content -LiteralPath (Join-Path $commandsTarget "$commandSlug.md") -Encoding utf8NoBOM
        $commandLinks += "- [``$command``]($commandSlug.md)"
    }
    @("# Script Commands", "", "Choose the exact DSL action name. Alias pages route to their shared behavior documentation.", "") +
        $commandLinks | Set-Content -LiteralPath (Join-Path $commandsTarget "index.md") -Encoding utf8NoBOM
}

function New-NavigationIndex([string]$Directory, [bool]$IsRoot = $false) {
    $indexName = if ($IsRoot) { "index.md" } else { "_navigation.md" }
    $entries = @("# Reference Navigation", "", "Follow these links to a category or leaf reference.", "")
    foreach ($child in (Get-ChildItem -LiteralPath $Directory -Directory | Sort-Object Name)) {
        New-NavigationIndex $child.FullName $false
        $entries += "- [$($child.Name)]($($child.Name)/_navigation.md)"
    }
    foreach ($file in (Get-ChildItem -LiteralPath $Directory -Filter '*.md' -File | Where-Object Name -ne $indexName | Sort-Object Name)) {
        $entries += "- [$($file.BaseName)]($($file.Name))"
    }
    $entries | Set-Content -LiteralPath (Join-Path $Directory $indexName) -Encoding utf8NoBOM
}

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
Remove-Item -LiteralPath $referenceRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $referenceRoot | Out-Null

Copy-Item -LiteralPath (Join-Path $PSScriptRoot "CMG.skill.md") -Destination (Join-Path $releaseRoot "SKILL.md") -Force
Copy-Item -LiteralPath (Join-Path $repositoryRoot "README.md") -Destination (Join-Path $referenceRoot "README.md") -Force

$agentsTarget = Join-Path $releaseRoot "agents"
New-Item -ItemType Directory -Force -Path $agentsTarget | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot "agents\*") -Destination $agentsTarget -Recurse -Force

$docsTarget = Join-Path $referenceRoot "docs"
$demosTarget = Join-Path $referenceRoot "demo-scripts"
New-Item -ItemType Directory -Force -Path $docsTarget, $demosTarget | Out-Null
Copy-Item -Path (Join-Path $repositoryRoot "docs\*") -Destination $docsTarget -Recurse -Force
Copy-Item -Path (Join-Path $repositoryRoot "demo-scripts\*") -Destination $demosTarget -Recurse -Force
Export-ActionTopics `
    (Join-Path $docsTarget "scripting\actions.md") `
    (Join-Path $docsTarget "scripting\action-topics") `
    (Join-Path $repositoryRoot "Browser\Scripting\BrowserScriptRunner.ActionDispatch.cs")
New-NavigationIndex $referenceRoot $true

$skill = Get-Item -LiteralPath (Join-Path $releaseRoot "SKILL.md")
$skillLines = (Get-Content -LiteralPath $skill.FullName).Count
if ($skill.Length -gt 65536 -or $skillLines -gt 400) {
    throw "Generated SKILL.md must stay at or below 64 KiB and 400 lines; got $($skill.Length) bytes and $skillLines lines."
}

Write-Host "Generated compact SKILL.md ($($skill.Length) bytes, $skillLines lines) with on-demand references."
