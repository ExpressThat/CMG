# `browser control events dialogs wait`

Waits for a matching browser dialog.

```powershell
cmg browser control events dialogs wait "<text>" [--timeout <milliseconds>]
```

## Arguments

- `<text>`: Dialog message text to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
DIALOG 001 {"type":"alert","message":"ready"}
```

## Stderr

Writes browser, timeout, or action errors.

## Exit Codes

- `0`: A matching dialog was found.
- `1`: Browser is not running or the action failed.
