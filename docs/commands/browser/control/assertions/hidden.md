# `browser control assertions hidden`

Runs the scripting `expectHidden` action once from the command line.

```powershell
cmg browser control assertions hidden "<selector>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <ms>`: Poll until the element is hidden or the timeout expires.

## Stdout

```text
PASS 001 expectHidden #toast
EXPECT 001 hidden #toast
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element was hidden, detached, or missing.
- `1`: Browser is not running, the element stayed visible, or the timeout expired.

## Example

```powershell
cmg browser control assertions hidden "#toast"
```
