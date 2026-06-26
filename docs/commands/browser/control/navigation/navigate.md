# `browser control navigation navigate`

Runs the scripting `navigate` action once from the command line.

```powershell
cmg browser control navigation navigate "<url-or-path>"
```

## Arguments

- `<url-or-path>`: URL, data URL, or existing local file path.

## Stdout

```text
PASS 001 navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
```

## Stderr

Writes browser, path, or navigation errors.

## Exit Codes

- `0`: Navigation succeeded.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation navigate "C:\Projects\CMG\index.html"
```
