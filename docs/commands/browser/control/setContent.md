# `browser control setContent`

Runs the scripting `setContent` action once from the command line.

```powershell
cmg browser control setContent "<html>"
```

## Arguments

- `<html>`: HTML assigned to `document.documentElement.innerHTML`.

## Stdout

```text
PASS 001 setContent <main>Ready</main>
CONTENT_SET 001 length=18
```

## Stderr

Writes browser or evaluation errors.

## Exit Codes

- `0`: The page content was replaced.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control setContent "<main id='ready'>Ready</main>"
```
