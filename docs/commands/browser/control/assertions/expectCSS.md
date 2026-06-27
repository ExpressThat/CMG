# `browser control assertions expectCSS`

Runs the scripting `expectCSS` action once from the command line.

```powershell
cmg browser control assertions expectCSS "<selector>" "<property>" "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<property>`: CSS property name, such as `display` or `background-color`.
- `<expected>`: Expected computed style value or value fragment.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectCSS #save display block
EXPECT 001 css #save
```

## Stderr

Writes browser, selector, timeout, CSS mismatch, or action errors.

## Exit Codes

- `0`: Computed CSS property contained the expected value.
- `1`: Browser is not running, no element matched, CSS value did not match, or the action failed.

## Example

```powershell
cmg browser control assertions expectCSS "#save" "display" "block"
```
