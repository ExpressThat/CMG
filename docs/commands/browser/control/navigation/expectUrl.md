# `browser control navigation expectUrl`

Runs the scripting `expectUrl` action once from the command line.

```powershell
cmg browser control navigation expectUrl "<expected>"
```

## Arguments

- `<expected>`: URL substring expected in the current page URL.

## Stdout

```text
PASS 001 expectUrl checkout
URL 001 https://example.com/checkout
```

## Stderr

Writes browser, JavaScript, or URL mismatch errors.

## Exit Codes

- `0`: The current URL contained the expected text.
- `1`: Browser is not running, the URL did not match, or the action failed.

## Example

```powershell
cmg browser control navigation expectUrl "checkout"
```
