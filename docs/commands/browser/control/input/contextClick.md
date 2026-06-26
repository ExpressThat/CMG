# `browser control input contextClick`

Runs the scripting `contextClick` action once from the command line.

```powershell
cmg browser control input contextClick "<selector>"
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Stdout

```text
PASS 001 contextClick #save
MOUSE_EVENT 001 contextmenu #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Context click completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input contextClick "#save"
```
