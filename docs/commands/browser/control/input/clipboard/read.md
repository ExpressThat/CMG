# `browser control input clipboard read`

Reads page-side clipboard shim text.

```powershell
cmg browser control input clipboard read
```

## Stdout

```text
PASS 001 readClipboard
CLIPBOARD 001 hello
```

## Exit Codes

- `0`: Clipboard text was read.
- `1`: Browser is not running or the action failed.
