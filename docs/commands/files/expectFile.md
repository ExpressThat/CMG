# `files expectFile`

Asserts that a local file exists and optionally contains text. This is the CLI alias for the scripting `expectFile` action.

```powershell
cmg files expectFile --path <file> [--contains <text>]
```

## Options

- `--path <file>`: Required local file path.
- `--contains <text>`: Required text inside the file.

## Stdout

```text
FILE_OK 001 C:\path\result.txt
```

## Stderr

Writes the missing file path or the expected text that was not found.

## Exit Codes

- `0`: File exists and optional content matched.
- `1`: File was missing or content did not match.
