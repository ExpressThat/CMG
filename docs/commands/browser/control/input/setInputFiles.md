# `browser control input setInputFiles`

Runs the scripting `setInputFiles` action once from the command line.

```powershell
cmg browser control input setInputFiles "<selector>" <file> [<file>...]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator for an `input[type=file]`.
- `<file>`: One or more local files. Relative paths resolve from the current working directory.

## Stdout

```text
PASS 001 setInputFiles #avatar C:\Projects\CMG\fixtures\avatar.png
UPLOAD 001 1
```

## Stderr

Writes browser, selector, file, or action errors.

## Exit Codes

- `0`: Files were assigned and `input`/`change` events were dispatched.
- `1`: Browser is not running, no element matched, a file was missing, or the action failed.

## Example

```powershell
cmg browser control input setInputFiles "#avatar" fixtures/avatar.png
```
