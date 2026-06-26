# `browser control assertions expectClass`

Runs the scripting `expectClass` action once from the command line.

```powershell
cmg browser control assertions expectClass "<selector>" "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Class token or class-name fragment expected on the element.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectClass #save ready
EXPECT 001 class #save
```

## Stderr

Writes browser, selector, timeout, class mismatch, or action errors.

## Exit Codes

- `0`: Element class contained the expected value.
- `1`: Browser is not running, no element matched, class did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectClass "#save" "ready"
```
