# `browser control input clipboard set`

Sets page-side clipboard shim text.

```powershell
cmg browser control input clipboard set "<text>"
```

## Arguments

- `<text>`: Clipboard text.

## Stdout

```text
PASS 001 setClipboard hello
CLIPBOARD_SET 001 5
```

## Exit Codes

- `0`: Clipboard text was set.
- `1`: Browser is not running or the action failed.
