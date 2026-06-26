# `browser control frames frameAssertText`

Runs the scripting `frameAssertText` action once from the command line.

```powershell
cmg browser control frames frameAssertText "<frameSelector>" "<selector>" "<text>" [options]
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector inside the iframe.
- `<text>`: Expected text.

## Options

- `--match <contains|exact|regex>`: Text match mode. Default is `contains`.
- `--ignore-case`: Match frame text without case sensitivity.

## Stdout

```text
PASS 001 frameAssertText #frame #status Saved
FRAME 001 frameAssertText
```

## Stderr

Writes browser, frame, selector, assertion, parse, or action errors.

## Exit Codes

- `0`: Text matched.
- `1`: Browser is not running, the frame is missing, the regex is invalid, or the assertion failed.

## Examples

```powershell
cmg browser control frames frameAssertText "#checkoutFrame" "#status" "^Saved$" --match regex --ignore-case
```
