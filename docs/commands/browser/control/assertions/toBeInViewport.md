# `browser control assertions toBeInViewport`

Runs the scripting `toBeInViewport` action once from the command line.

```powershell
cmg browser control assertions toBeInViewport "<selector>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectInViewport`](expectInViewport.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toBeInViewport #save
EXPECT 001 inviewport #save
```

## Stderr

Writes browser, selector, timeout, viewport-intersection, or action errors.

## Exit Codes

- `0`: Element intersected the viewport.
- `1`: Browser is not running, no element matched, element was outside the viewport, or the action failed.

## Example

```powershell
cmg browser control assertions toBeInViewport "#save"
```
