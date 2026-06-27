# `browser control events console wait`

Waits for a matching console message.

```powershell
cmg browser control events console wait "<text>" [options]
```

## Arguments

- `<text>`: Console message text to match.

## Options

- `--level <level>`: Console level filter: `log`, `info`, `warn`, or `error`.
- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
CONSOLE 001 log: ready
```

## Stderr

Writes browser, level, timeout, or action errors.

## Exit Codes

- `0`: A matching console message was found.
- `1`: Browser is not running or the action failed.
