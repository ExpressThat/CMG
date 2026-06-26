# `browser control assertions expectAttribute`

Runs the scripting `expectAttribute` action once from the command line.

```powershell
cmg browser control assertions expectAttribute "<selector>" "<name>" "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<name>`: Attribute name.
- `<expected>`: Expected attribute value fragment.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 expectAttribute #save data-state ready
EXPECT 001 attribute #save
```

## Stderr

Writes browser, selector, timeout, attribute mismatch, or action errors.

## Exit Codes

- `0`: Attribute contained the expected text before the timeout.
- `1`: Browser is not running, no element matched, attribute did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectAttribute "#save" "data-state" "ready"
```
