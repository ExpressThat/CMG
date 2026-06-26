# `browser control frames assertText`

Asserts text inside an iframe element.

```powershell
cmg browser control frames assertText "<frameSelector>" "<selector>" "<text>" [options]
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

## Exit Codes

- `0`: Expected text was found.
- `1`: Browser is not running, the frame is missing, the regex is invalid, or the assertion failed.

## Examples

```powershell
cmg browser control frames assertText "#checkoutFrame" "#status" "^Saved$" --match regex --ignore-case
```
