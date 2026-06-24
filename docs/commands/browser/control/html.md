# `browser control html`

Runs the scripting `html` action once from the command line.

```powershell
cmg browser control html "<selector>"
```

## Arguments

- `<selector>`: CSS selector.

## Stdout

Prints a `PASS` line and an `HTML` result line:

```text
PASS 001 html #openProfileDialog
HTML 001 <button id="openProfileDialog" type="button">Open profile dialog</button>
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: HTML was printed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control html "#openProfileDialog"
```
