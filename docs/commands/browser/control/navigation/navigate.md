# `browser control navigation navigate`

Runs the scripting `navigate` action once from the command line.

```powershell
cmg browser control navigation navigate "<url-or-path>" [--wait-until <state>] [--timeout <milliseconds>]
```

## Arguments

- `<url-or-path>`: URL, data URL, or existing local file path.

## Options

- `--wait-until <state>`: Optional post-navigation state to wait for. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.
- `--timeout <milliseconds>`: Maximum wait time when `--wait-until` waits for a page state. Default is `5000`.

## Stdout

```text
PASS 001 navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
```

With `--wait-until`, stdout includes the requested wait and final document state:

```text
PASS 001 navigate https://example.com waitUntil=domcontentloaded timeout=5000
NAVIGATED 001 https://example.com/ waitUntil=domcontentloaded state=interactive
```

## Stderr

Writes browser, path, or navigation errors.

## Exit Codes

- `0`: Navigation succeeded.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation navigate "C:\Projects\CMG\index.html"
cmg browser control navigation navigate "https://example.com" --wait-until networkidle --timeout 10000
```
