# `browser control frames waitForElement`

Waits for an element inside a same-origin iframe.

```powershell
cmg browser control frames waitForElement "<frameSelector>" "<selector>" [--timeout <milliseconds>]
```

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 frameWaitForElement #frame #ready timeout=5000
FRAME 001 frameWaitForElement
```

## Exit Codes

- `0`: Element was found.
- `1`: Browser is not running or the action failed.
