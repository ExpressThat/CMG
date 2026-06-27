# `browser control assertions toBeChecked`

Runs the scripting `toBeChecked` action once from the command line.

```powershell
cmg browser control assertions toBeChecked "<selector>" [--expected <true|false>] [--timeout <milliseconds>]
```

This is a Playwright-style alias for [`expectChecked`](expectChecked.md).

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--expected <true|false>`: Expected checked state. Defaults to `true`.
- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 toBeChecked #agree
EXPECT 001 checked #agree
```

## Stderr

Writes browser, selector, timeout, checked-state mismatch, or action errors.

## Exit Codes

- `0`: Checked state matched before the timeout.
- `1`: Browser is not running, no element matched, checked state did not match, or the action failed.

## Example

```powershell
cmg browser control assertions toBeChecked "#agree" --expected false
```
