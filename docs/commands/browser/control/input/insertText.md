# `browser control input insertText`

Runs the scripting `insertText` action once from the command line.

```powershell
cmg browser control input insertText "<text>"
```

## Arguments

- `<text>`: Text to insert at the active element.

## Stdout

```text
PASS 001 insertText hello
TEXT_INSERTED 001 5
```

## Stderr

Writes browser or text insertion errors.

## Exit Codes

- `0`: Text insertion was requested.
- `1`: Browser is not running or the action failed.
