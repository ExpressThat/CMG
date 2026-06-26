# `browser control input clipboard writeClipboard`

Runs the scripting `writeClipboard` action once from the command line.

```powershell
cmg browser control input clipboard writeClipboard "<text>"
```

## Arguments

- `<text>`: Clipboard text.

## Stdout

```text
PASS 001 writeClipboard hello
CLIPBOARD_SET 001 5
```

## Stderr

Writes browser, parse, or action errors.

## Exit Codes

- `0`: Clipboard text was set.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input clipboard writeClipboard "hello"
```
