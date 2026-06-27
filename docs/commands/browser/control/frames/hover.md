# `browser control frames hover`

Hovers an element inside a same-origin iframe.

```powershell
cmg browser control frames hover "<frameSelector>" "<selector>"
```

## Stdout

```text
PASS 001 frameHover #frame #help
FRAME 001 frameHover
```

## Exit Codes

- `0`: Element was hovered.
- `1`: Browser is not running or the action failed.
