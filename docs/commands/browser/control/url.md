# `browser control url`

Runs the scripting `url` action once from the command line.

```powershell
cmg browser control url
```

## Stdout

```text
PASS 001 url
URL 001 https://example.com/
```

## Stderr

Writes browser or evaluation errors.

## Exit Codes

- `0`: The current URL was read.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control url
```
