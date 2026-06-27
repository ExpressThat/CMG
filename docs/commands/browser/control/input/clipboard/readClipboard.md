# `browser control input clipboard readClipboard`

Runs the scripting `readClipboard` action once from the command line.

```powershell
cmg browser control input clipboard readClipboard
```

## Stdout

```text
PASS 001 readClipboard
CLIPBOARD 001 hello
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Clipboard text was read.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input clipboard readClipboard
```
