# `browser control click`

Runs the scripting `click` action once from the command line.

```powershell
cmg browser control click "<selector>"
```

## Arguments

- `<selector>`: CSS selector for the element to click.

## Stdout

```text
PASS 001 click #openProfileDialog
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Click completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control click "#openProfileDialog"
```
