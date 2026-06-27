# `browser control navigation expectTitle`

Runs the scripting `expectTitle` action once from the command line.

```powershell
cmg browser control navigation expectTitle "<expected>" [--match <mode>] [--ignore-case]
```

## Arguments

- `<expected>`: Title text expected in the current page title.

## Options

- `--match <mode>`: Match mode: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Use case-insensitive matching.

## Stdout

```text
PASS 001 expectTitle Checkout
TITLE 001 Checkout
```

## Stderr

Writes browser, JavaScript, option, regex, or title mismatch errors.

## Exit Codes

- `0`: The current title matched the expected text.
- `1`: Browser is not running, the title did not match, or the action failed.

## Example

```powershell
cmg browser control navigation expectTitle "Checkout" --match exact
cmg browser control navigation expectTitle "checkout" --ignore-case
```
