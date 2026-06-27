# `browser control assertions expectInViewport`

Runs the scripting `expectInViewport` action once from the command line.

```powershell
cmg browser control assertions expectInViewport "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectInViewport #save
EXPECT 001 inviewport #save
```

## Stderr

Writes browser, selector, timeout, viewport-intersection, or action errors.

## Exit Codes

- `0`: Element intersected the viewport.
- `1`: Browser is not running, no element matched, element was outside the viewport, or the action failed.

## Example

```powershell
cmg browser control assertions expectInViewport "#save"
```
