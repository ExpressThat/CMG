# `browser control navigation toHaveURL`

Runs the scripting `toHaveURL` action once from the command line.

```powershell
cmg browser control navigation toHaveURL "<expected>" [--match <mode>] [--ignore-case]
```

## Arguments

- `<expected>`: URL text expected in the current page URL.

## Options

- `--match <mode>`: Match mode: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Use case-insensitive matching.

## Stdout

```text
PASS 001 toHaveURL checkout
URL 001 https://example.com/checkout
```

## Stderr

Writes browser, JavaScript, option, regex, or URL mismatch errors.

## Exit Codes

- `0`: The current URL matched the expected text.
- `1`: Browser is not running, the URL did not match, or the action failed.

## Example

```powershell
cmg browser control navigation toHaveURL "checkout" --match regex --ignore-case
```
