# `browser control input tap`

Runs the scripting `tap` action once from the command line.

```powershell
cmg browser control input tap "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 tap #touchTarget
TAP 001 #touchTarget
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Touch-style pointer events were dispatched.
- `1`: Browser is not running or the action failed.
