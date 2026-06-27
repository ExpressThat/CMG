# `browser control navigation goto`

Runs the scripting `goto` action once from the command line.

```powershell
cmg browser control navigation goto "<target>" [--wait-until <state>] [--timeout <milliseconds>]
```

## Arguments

- `<target>`: URL, data URL, or local file path.

## Options

- `--wait-until <state>`: Optional post-navigation state to wait for. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.
- `--timeout <milliseconds>`: Maximum wait time when `--wait-until` waits for a page state. Default is `5000`.

## Stdout

```text
PASS 001 goto https://example.com
NAVIGATED 001 https://example.com/
```

With `--wait-until`, stdout includes the requested wait and final document state:

```text
PASS 001 goto https://example.com waitUntil=load timeout=5000
NAVIGATED 001 https://example.com/ waitUntil=load state=complete
```

## Stderr

Writes browser, navigation, parse, or action errors.

## Exit Codes

- `0`: Navigation completed.
- `1`: Browser is not running or navigation failed.

## Examples

```powershell
cmg browser control navigation goto "https://example.com"
cmg browser control navigation goto "https://example.com" --wait-until load
```
