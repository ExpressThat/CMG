# `browser control input hover`

Runs the scripting `hover` action once from the command line.

```powershell
cmg browser control input hover "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 hover #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Hover events were dispatched.
- `1`: Browser is not running or the action failed.
