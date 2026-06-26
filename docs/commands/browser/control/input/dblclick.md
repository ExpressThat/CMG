# `browser control input dblclick`

Runs the scripting `dblclick` action once from the command line.

```powershell
cmg browser control input dblclick "<selector>"
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Stdout

```text
PASS 001 dblclick #save
MOUSE_EVENT 001 dblclick #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Double-click completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input dblclick "#save"
```
