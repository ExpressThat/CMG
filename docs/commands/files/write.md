# `files write`

Writes text to a local file, replacing existing content.

```powershell
cmg files write --path <file> "<text>"
```

## Options

- `--path <file>`: Required local file path.

## Stdout

```text
FILE_WRITTEN 001 C:\path\result.txt
```

## Exit Codes

- `0`: File was written.
- `1`: File could not be written.
