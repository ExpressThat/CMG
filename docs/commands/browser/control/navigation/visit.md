# `browser control navigation visit`

Runs the scripting `visit` action once from the command line.

```powershell
cmg browser control navigation visit "<target>"
```

## Arguments

- `<target>`: URL, data URL, or local file path.

## Stdout

```text
PASS 001 visit https://example.com
NAVIGATED 001 https://example.com/
```

## Stderr

Writes browser, navigation, parse, or action errors.

## Exit Codes

- `0`: Navigation completed.
- `1`: Browser is not running or navigation failed.

## Examples

```powershell
cmg browser control navigation visit "https://example.com"
```
