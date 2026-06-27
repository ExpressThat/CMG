# `files append`

Appends text to a local file.

```powershell
cmg files append --path <file> "<text>"
```

## Options

- `--path <file>`: Required local file path.

## Stdout

```text
FILE_APPENDED 001 C:\path\result.txt
```

## Exit Codes

- `0`: File was appended.
- `1`: File could not be written.
