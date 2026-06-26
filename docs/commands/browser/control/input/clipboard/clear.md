# `browser control input clipboard clear`

Clears page-side clipboard shim text.

```powershell
cmg browser control input clipboard clear
```

## Stdout

```text
PASS 001 clearClipboard
CLIPBOARD_CLEARED 001
```

## Exit Codes

- `0`: Clipboard text was cleared.
- `1`: Browser is not running or the action failed.
