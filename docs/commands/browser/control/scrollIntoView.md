# `browser control scrollIntoView`

Runs the scripting `scrollIntoView` action once from the command line.

```powershell
cmg browser control scrollIntoView "<selector>"
```

## Arguments

- `<selector>`: CSS selector.

## Stdout

```text
PASS 001 scrollIntoView #dragdrop
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Element was scrolled into view.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control scrollIntoView "#dragdrop"
```
