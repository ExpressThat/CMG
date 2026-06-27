# `browser control tabs openTab`

Runs the scripting `openTab` action once from the command line.

```powershell
cmg browser control tabs openTab "<target>"
```

## Arguments

- `<target>`: URL, data URL, or local file path to open in a new tab.

## Stdout

```text
PASS 001 openTab about:blank
TAB_OPENED 001 about:blank
```

## Stderr

Writes browser, navigation, parse, or action errors.

## Exit Codes

- `0`: Tab was opened.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control tabs openTab "about:blank"
```
