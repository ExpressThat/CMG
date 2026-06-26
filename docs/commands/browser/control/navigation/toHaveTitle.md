# `browser control navigation toHaveTitle`

Runs the scripting `toHaveTitle` action once from the command line.

```powershell
cmg browser control navigation toHaveTitle "<expected>"
```

## Arguments

- `<expected>`: Title substring expected in the current page title.

## Stdout

```text
PASS 001 toHaveTitle Checkout
TITLE 001 Checkout
```

## Stderr

Writes browser, JavaScript, or title mismatch errors.

## Exit Codes

- `0`: The current title contained the expected text.
- `1`: Browser is not running, the title did not match, or the action failed.

## Example

```powershell
cmg browser control navigation toHaveTitle "Checkout"
```
