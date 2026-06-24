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

Writes browser, missing-element, or offscreen-element errors. `click` does not scroll automatically; run `scrollIntoView` first when the page should move.

## Exit Codes

- `0`: Click completed.
- `1`: Browser is not running, no element matched, the element is outside the current viewport, or the action failed.

## Example

```powershell
cmg browser control click "#openProfileDialog"
```
