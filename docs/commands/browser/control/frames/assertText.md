# `browser control frames assertText`

Asserts text inside an iframe element.

```powershell
cmg browser control frames assertText "<frameSelector>" "<selector>" "<text>"
```

## Arguments

- `<text>`: Expected text fragment.

## Stdout

```text
PASS 001 frameAssertText #frame #status Saved
FRAME 001 frameAssertText
```

## Exit Codes

- `0`: Expected text was found.
- `1`: Browser is not running or the assertion failed.
