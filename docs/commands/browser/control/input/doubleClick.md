# `browser control input doubleClick`

Runs the scripting `doubleClick` action once from the command line.

```powershell
cmg browser control input doubleClick "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 doubleClick #save
MOUSE_EVENT 001 dblclick #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The double-click event was dispatched.
- `1`: Browser is not running or the action failed.
