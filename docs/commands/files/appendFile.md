# `files appendFile`

Appends text to a local file. This is the CLI alias for the scripting `appendFile` action.

```powershell
cmg files appendFile --path <file> "<text>"
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
