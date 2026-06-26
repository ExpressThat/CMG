# `browser control tabs open`

Runs the scripting `openTab` action once from the command line.

```powershell
cmg browser control tabs open "<target>"
```

## Arguments

- `<target>`: URL, data URL, or local file path to open in a new tab.

## Stdout

```text
PASS 001 openTab https://example.com
TAB_OPENED 001 https://example.com
```

## Stderr

Writes browser, path, or tab-opening errors.

## Exit Codes

- `0`: A new tab was requested.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control tabs open "https://example.com"
```
