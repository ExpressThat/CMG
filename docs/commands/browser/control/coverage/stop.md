# `browser control coverage stop`

Stops coverage collection and prints or writes coverage JSON.

```powershell
cmg browser control coverage stop [--path <file>]
```

## Options

- `--path <file>`: Write coverage JSON to this path instead of stdout.

## Stdout

Without `--path`, stdout includes the JSON payload:

```text
PASS 001 stopCoverage
COVERAGE 001 {"js":[],"css":[]}
```

With `--path`, stdout includes the written file path:

```text
COVERAGE 001 C:\path\coverage.json
```

## Exit Codes

- `0`: Coverage collection stopped.
- `1`: Browser is not running or the action failed.
