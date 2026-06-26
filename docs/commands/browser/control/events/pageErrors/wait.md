# `browser control events pageErrors wait`

Waits for a matching page error.

```powershell
cmg browser control events pageErrors wait "<text>" [--timeout <milliseconds>]
```

## Arguments

- `<text>`: Error text to match.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PAGE_ERROR 001 Error: boom
```

## Stderr

Writes browser, timeout, or action errors.

## Exit Codes

- `0`: A matching page error was found.
- `1`: Browser is not running or the action failed.
