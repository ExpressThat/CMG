# `browser control frames type`

Types text into an element inside a same-origin iframe.

```powershell
cmg browser control frames type "<frameSelector>" "<selector>" "<text>"
```

## Arguments

- `<text>`: Text to append.

## Stdout

```text
PASS 001 frameType #frame #email agent@example.com
FRAME 001 frameType
```

## Exit Codes

- `0`: Text was typed.
- `1`: Browser is not running or the action failed.
