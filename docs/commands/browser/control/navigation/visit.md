# `browser control navigation visit`

Runs the scripting `visit` action once from the command line.

```powershell
cmg browser control navigation visit "<target>" [--wait-until <state>] [--timeout <milliseconds>]
```

## Arguments

- `<target>`: URL, data URL, or local file path.

## Options

- `--wait-until <state>`: Optional post-navigation state to wait for. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.
- `--timeout <milliseconds>`: Maximum wait time when `--wait-until` waits for a page state. Default is `5000`.

## Stdout

```text
PASS 001 visit https://example.com
NAVIGATED 001 https://example.com/
```

With `--wait-until`, stdout includes the requested wait and final document state:

```text
PASS 001 visit https://example.com waitUntil=networkidle timeout=10000
NAVIGATED 001 https://example.com/ waitUntil=networkidle state=complete
```

## Stderr

Writes browser, navigation, parse, or action errors.

## Exit Codes

- `0`: Navigation completed.
- `1`: Browser is not running or navigation failed.

## Examples

```powershell
cmg browser control navigation visit "https://example.com"
cmg browser control navigation visit "https://example.com" --wait-until networkidle --timeout 10000
```
