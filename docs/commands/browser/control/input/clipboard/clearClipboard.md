# `browser control input clipboard clearClipboard`

Runs the scripting `clearClipboard` action once from the command line.

```powershell
cmg browser control input clipboard clearClipboard
```

## Stdout

```text
PASS 001 clearClipboard
CLIPBOARD_CLEARED 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Clipboard text was cleared.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input clipboard clearClipboard
```
