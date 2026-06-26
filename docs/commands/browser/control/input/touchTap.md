# `browser control input touchTap`

Runs the scripting `touchTap` action once from the command line.

```powershell
cmg browser control input touchTap "<selector>"
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Stdout

```text
PASS 001 touchTap #save
TAP 001 #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Tap completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input touchTap "#save"
```
