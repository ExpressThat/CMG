# `browser control navigation goto`

Runs the scripting `goto` action once from the command line.

```powershell
cmg browser control navigation goto "<target>"
```

## Arguments

- `<target>`: URL, data URL, or local file path.

## Stdout

```text
PASS 001 goto https://example.com
NAVIGATED 001 https://example.com/
```

## Stderr

Writes browser, navigation, parse, or action errors.

## Exit Codes

- `0`: Navigation completed.
- `1`: Browser is not running or navigation failed.

## Examples

```powershell
cmg browser control navigation goto "https://example.com"
```
