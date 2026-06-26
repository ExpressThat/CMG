# `files fixture`

Reads a local file. This is the CLI alias for the scripting `fixture` action.

```powershell
cmg files fixture --path <file> [--encoding base64]
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

Unlike scripting `fixture`, the CLI command does not assign a variable. Agent callers should parse the `FILE_BODY` line.

## Exit Codes

- `0`: File was read.
- `1`: File was missing or could not be read.
