# `browser control navigate`

Runs the scripting `navigate` action once from the command line.

```powershell
cmg browser control navigate "<url-or-path>"
```

## Arguments

- `<url-or-path>`: URL, data URL, or existing local file path.

## Stdout

Writes the same one-action script log as `browser control script`:

```text
PASS 001 navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
```

## Stderr

Writes browser, parse, or action errors. Invalid browser URLs and missing local path targets fail with a non-zero exit code instead of logging a successful navigation.

## Exit Codes

- `0`: Navigation succeeded.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigate "C:\Projects\CMG\index.html"
```
