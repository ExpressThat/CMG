# `browser control frames frameToHaveText`

Asserts text inside an iframe element using the `frameToHaveText` script action.

```powershell
cmg browser control frames frameToHaveText "<frameSelector>" "<selector>" "<text>" [options]
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich/provider locator inside the iframe.
- `<text>`: Expected text.

## Options

- `--match <contains|exact|regex>`: Text match mode. Default is `contains`.
- `--ignore-case`: Match frame text without case sensitivity.

## Stdout

```text
PASS 001 frameToHaveText #frame #status Saved
FRAME 001 frameToHaveText
```

## Stderr

Frame, selector, regex, timeout, and assertion failures are written to stderr with the action name and reason.

## Exit Codes

- `0`: Expected text was found.
- `1`: Browser is not running, the frame is missing, the regex is invalid, or the assertion failed.

## Examples

```powershell
cmg browser control frames frameToHaveText "#checkoutFrame" "#status" "Saved" --match exact
```
