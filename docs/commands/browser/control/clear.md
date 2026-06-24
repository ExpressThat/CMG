# `browser control clear`

Runs the scripting `clear` action once from the command line.

```powershell
cmg browser control clear "<selector>"
```

## Arguments

- `<selector>`: CSS selector for the input-like element.

## Stdout

```text
PASS 001 clear #profileName
```

## Stderr

Writes browser, missing-element, or offscreen-element errors. `clear` focuses without scrolling; run `scrollIntoView` first when the page should move.

## Exit Codes

- `0`: Element value was cleared.
- `1`: Browser is not running, no element matched, the element is outside the current viewport, or the action failed.

## Example

```powershell
cmg browser control clear "#profileName"
```
