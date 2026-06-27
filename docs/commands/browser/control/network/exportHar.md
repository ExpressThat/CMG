# `browser control network exportHar`

Runs the scripting `exportHar` action once from the command line.

```powershell
cmg browser control network exportHar --path <path>
```

## Options

- `--path <path>`: Required HAR output path.

## Stdout

```text
PASS 001 exportHar
HAR_EXPORTED 001 C:\Projects\CMG\artifacts\network.har
```

## Exit Codes

- `0`: HAR file was written.
- `1`: Browser is not running or the action failed.
