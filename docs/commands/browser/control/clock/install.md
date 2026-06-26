# `browser control clock install`

Installs deterministic page-side time control.

```powershell
cmg browser control clock install [--now <epoch-ms>]
```

## Options

- `--now <epoch-ms>`: Epoch milliseconds. Default is current host time.

## Stdout

```text
PASS 001 clock now=1700000000000
CLOCK 001 1700000000000
```

## Exit Codes

- `0`: Clock was installed.
- `1`: Browser is not running or the action failed.
