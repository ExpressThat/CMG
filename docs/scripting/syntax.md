# `.cmgscript` Syntax

Each executable line has this shape:

```text
action positionalArgs... key=value...
```

## Comments And Blank Lines

Blank lines are ignored.

Lines beginning with `#` are comments:

```text
# Open the local test page
navigate "C:\Projects\CMG\index.html"
```

Only full-line comments are supported. Inline comments are treated as arguments.

## Quoting

Use quotes for values that contain spaces, `#`, brackets, or shell-sensitive characters:

```text
click "#openProfileDialog"
type "#profileName" "CMG Test Profile"
```

Escaped quotes inside quoted strings use `\"`:

```text
type "#releaseNote" "Text with a \"quoted\" word"
```

Backslashes are kept as literal characters unless they escape `"` or another `\`, so Windows paths work as expected:

```text
navigate "C:\Projects\CMG\index.html"
```

## Arguments

Positional arguments come after the action name:

```text
waitForElement "#profileDialog[open]"
```

Keyed options use `key=value`:

```text
waitForElement "#profileDialog[open]" timeout=5000
screenshot "#profileDialog" output="profile-dialog.png"
```

Only identifier-like keys are treated as options. CSS selectors that contain `=`, such as `[data-command='browser launch']`, remain normal positional arguments.

Durations are milliseconds.

## Variables

Set variables with:

```text
set profileName "CMG Test Profile"
```

Reference variables with `${name}`:

```text
type "#profileName" "${profileName}"
```

Variables are expanded before each action runs. Referencing an undefined variable fails the script.

## Paths And URLs

`navigate` accepts URLs and existing local file paths. Existing file paths are converted to `file:///` URLs.

```text
navigate "https://example.com"
navigate "C:\Projects\CMG\index.html"
navigate "data:text/html,<h1>Hello</h1>"
```

Screenshot `output=` paths are written as files. Parent directories are created when needed.
