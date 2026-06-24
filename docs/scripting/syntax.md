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

## Blocks

Some actions can take a nested block body:

```text
action positionalArgs... {
  childAction positionalArgs...
}
```

The opening `{` must be at the end of the parent action line. The closing `}` must be on its own line.

For v1, block syntax is supported by the complex `dragAndDrop` action:

```text
dragAndDrop "[data-command='browser launch']" {
  delay 200
  hover "#lastDialogAction"
  drop "#dropQueue"
}
```

Rules:

- Block bodies use the same quoting, option, and variable expansion rules as top-level actions.
- Blank lines and full-line comments are allowed inside blocks.
- Nested blocks are parsed, but only actions that explicitly document block support may use them.
- `dragAndDrop` block bodies must contain exactly one `drop` action.
- No actions are allowed after `drop` in a `dragAndDrop` block.

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
