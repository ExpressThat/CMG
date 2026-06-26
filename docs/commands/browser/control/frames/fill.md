# `browser control frames fill`

Fills an element inside a same-origin iframe.

```powershell
cmg browser control frames fill "<frameSelector>" "<selector>" "<text>"
```

## Arguments

- `<text>`: Replacement text.

## Stdout

```text
PASS 001 frameFill #frame #email agent@example.com
FRAME 001 frameFill
```

## Exit Codes

- `0`: Element value was filled.
- `1`: Browser is not running or the action failed.
