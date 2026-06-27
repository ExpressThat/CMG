# `browser control frames click`

Clicks an element inside a same-origin iframe.

```powershell
cmg browser control frames click "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the iframe.
- `<selector>`: CSS selector or CMG rich/provider locator inside the iframe.

## Stdout

```text
PASS 001 frameClick #frame #save
FRAME 001 frameClick
```

## Exit Codes

- `0`: Element was clicked.
- `1`: Browser is not running or the action failed.
