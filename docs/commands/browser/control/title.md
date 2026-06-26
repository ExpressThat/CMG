# `browser control title`

Runs the scripting `title` action once from the command line.

```powershell
cmg browser control title
```

## Stdout

```text
PASS 001 title
TITLE 001 Checkout
```

## Stderr

Writes browser or evaluation errors.

## Exit Codes

- `0`: The current page title was read.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control title
```
