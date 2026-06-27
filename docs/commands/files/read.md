# `files read`

Reads a local file.

```powershell
cmg files read --path <file> [--encoding base64]
```

## Options

- `--path <file>`: Required local file path.
- `--encoding base64`: Return file bytes as base64 text.

## Stdout

```text
FILE_READ 001 C:\path\payload.json
FILE_BODY 001 {"ok":true}
```

## Exit Codes

- `0`: File was read.
- `1`: File was missing or could not be read.
