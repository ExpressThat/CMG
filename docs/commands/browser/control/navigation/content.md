# `browser control navigation content`

Runs the scripting `content` action once from the command line.

```powershell
cmg browser control navigation content
```

## Stdout

```text
PASS 001 content
CONTENT 001 <html>...</html>
```

## Stderr

Writes browser or evaluation errors.

## Exit Codes

- `0`: The current page HTML was read.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control navigation content
```
