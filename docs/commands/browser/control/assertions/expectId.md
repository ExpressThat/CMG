# `browser control assertions expectId`

Runs the scripting `expectId` action once from the command line.

```powershell
cmg browser control assertions expectId "<selector>" "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Expected exact element id.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectId #save save
EXPECT 001 id #save
```

## Stderr

Writes browser, selector, timeout, id mismatch, or action errors.

## Exit Codes

- `0`: Element id matched the expected value.
- `1`: Browser is not running, no element matched, id did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectId "#save" "save"
```
