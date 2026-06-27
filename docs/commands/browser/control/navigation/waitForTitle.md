# `browser control navigation waitForTitle`

Runs the scripting `waitForTitle` action once from the command line.

```powershell
cmg browser control navigation waitForTitle "<expected>" [--timeout <milliseconds>] [--match <mode>] [--ignore-case]
```

## Arguments

- `<expected>`: Title text to wait for.

## Options

- `--timeout <milliseconds>`: Maximum wait time. Defaults to `5000`.
- `--match <mode>`: Match mode: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Use case-insensitive matching.

## Stdout

```text
PASS 001 waitForTitle Checkout
TITLE 001 Checkout - Profile
```

## Stderr

Writes browser, timeout, JavaScript, option, regex, or title mismatch errors.

## Exit Codes

- `0`: Current page title matched the expected text before the timeout.
- `1`: Browser is not running, the title did not match before the timeout, or the action failed.

## Example

```powershell
cmg browser control navigation waitForTitle "Checkout" --match exact --timeout 5000
```
