# `files expect`

Asserts that a local file exists and optionally contains text.

```powershell
cmg files expect --path <file> [--contains <text>]
```

## Options

- `--path <file>`: Required local file path.
- `--contains <text>`: Required text inside the file.

## Stdout

```text
FILE_OK 001 C:\path\result.txt
```

## Exit Codes

- `0`: File exists and optional content matched.
- `1`: File was missing or content did not match.
