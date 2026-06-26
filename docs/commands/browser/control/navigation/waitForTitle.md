# `browser control navigation waitForTitle`

Runs the scripting `waitForTitle` action once from the command line.

```powershell
cmg browser control navigation waitForTitle "<expected>" [--timeout <milliseconds>]
```

## Arguments

- `<expected>`: Title substring to wait for.

## Options

- `--timeout <milliseconds>`: Maximum wait time. Defaults to `5000`.

## Stdout

```text
PASS 001 waitForTitle Checkout
TITLE 001 Checkout - Profile
```

## Stderr

Writes browser, timeout, JavaScript, or title mismatch errors.

## Exit Codes

- `0`: Current page title contained the expected text before the timeout.
- `1`: Browser is not running, the title did not match before the timeout, or the action failed.

## Example

```powershell
cmg browser control navigation waitForTitle "Checkout" --timeout 5000
```
