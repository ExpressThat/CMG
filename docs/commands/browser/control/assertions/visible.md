# `browser control assertions visible`

Runs the scripting `expectVisible` action once from the command line.

```powershell
cmg browser control assertions visible "<selector>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator such as `text=Save`.

## Options

- `--timeout <ms>`: Poll until the element is visible or the timeout expires.

## Stdout

```text
PASS 001 expectVisible #save
EXPECT 001 visible #save
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element was visible.
- `1`: Browser is not running, no element matched, element was not visible, or the timeout expired.

## Example

```powershell
cmg browser control assertions visible "#save" --timeout 5000
```
