# `browser control assertions toHaveClass`

Runs the scripting `toHaveClass` action once from the command line.

```powershell
cmg browser control assertions toHaveClass "<selector>" "<expected>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectClass`](expectClass.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Class token or class-name fragment expected on the element.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toHaveClass #save ready
EXPECT 001 class #save
```

## Stderr

Writes browser, selector, timeout, class mismatch, or action errors.

## Exit Codes

- `0`: Element class contained the expected value.
- `1`: Browser is not running, no element matched, class did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveClass "#save" "ready"
```
