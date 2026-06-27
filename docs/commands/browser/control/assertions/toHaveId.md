# `browser control assertions toHaveId`

Runs the scripting `toHaveId` action once from the command line.

```powershell
cmg browser control assertions toHaveId "<selector>" "<expected>" [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectId`](expectId.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected exact element id.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 toHaveId #save save
EXPECT 001 id #save
```

## Stderr

Writes browser, selector, timeout, id mismatch, or action errors.

## Exit Codes

- `0`: Element id matched the expected value.
- `1`: Browser is not running, no element matched, id did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toHaveId "#save" "save"
```
