# `files readFile`

Reads a local file. This is the CLI alias for the scripting `readFile` action.

```powershell
cmg files readFile --path <file> [--encoding base64]
```

## Options

- `--path <file>`: Required local file path.
- `--encoding base64`: Return file bytes as base64 text.

## Stdout

```text
FILE_READ 001 C:\path\payload.json
FILE_BODY 001 {"ok":true}
```

## Behavior

Unlike scripting `readFile`, the CLI command does not assign a variable. Agent callers should parse the `FILE_BODY` line.

## Exit Codes

- `0`: File was read.
- `1`: File was missing or could not be read.
