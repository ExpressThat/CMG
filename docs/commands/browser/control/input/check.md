# `browser control input check`

Runs the scripting `check` action once from the command line.

```powershell
cmg browser control input check "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 check #subscribe
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was checked.
- `1`: Browser is not running or the action failed.
