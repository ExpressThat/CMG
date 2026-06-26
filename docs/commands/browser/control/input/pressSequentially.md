# `browser control input pressSequentially`

Runs the scripting `pressSequentially` action once from the command line.

```powershell
cmg browser control input pressSequentially "<selector>" "<text>"
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<text>`: Text to enter.

## Stdout

```text
PASS 001 pressSequentially #name CMG
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Text was typed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input pressSequentially "#name" "CMG"
```
