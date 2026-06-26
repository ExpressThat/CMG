# `browser control input uploadFiles`

Assigns files to an `input[type=file]` element and dispatches `input` and `change` events.

```powershell
cmg browser control input uploadFiles "<selector>" <file> [file...]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator for the file input.
- `<file>`: One or more local files. Relative paths resolve from the current working directory.

## Stdout

```text
PASS 001 uploadFiles #avatar C:\path\avatar.png
UPLOAD 001 1
```

## Stderr

Writes browser, selector, missing-file, parse, or action errors.

## Exit Codes

- `0`: Files were assigned.
- `1`: Browser is not running or the action failed.
