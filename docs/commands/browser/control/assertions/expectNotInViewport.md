# `browser control assertions expectNotInViewport`

Runs the scripting `expectNotInViewport` action once from the command line.

```powershell
cmg browser control assertions expectNotInViewport "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotInViewport #offscreen
EXPECT 001 notinviewport #offscreen
```

## Stderr

Writes browser, selector, timeout, viewport-intersection mismatch, or action errors.

## Exit Codes

- `0`: Element did not intersect the viewport.
- `1`: Browser is not running, no element matched, element intersected the viewport, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotInViewport "#offscreen"
```
