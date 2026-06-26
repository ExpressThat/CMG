# `browser control input rightClick`

Runs the scripting `rightClick` action once from the command line.

```powershell
cmg browser control input rightClick "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 rightClick #menu
MOUSE_EVENT 001 contextmenu #menu
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The context-menu event was dispatched.
- `1`: Browser is not running or the action failed.
