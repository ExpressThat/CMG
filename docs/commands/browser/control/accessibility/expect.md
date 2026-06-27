# `browser control accessibility expect`

Asserts that an accessible element exists.

```powershell
cmg browser control accessibility expect --role <role> [--name <text>]
```

## Options

- `--role <role>`: Required accessible role.
- `--name <text>`: Optional text expected in the accessible name.

## Stdout

```text
PASS 001 expectAccessible role=button name=Save
ACCESSIBLE 001 role=button name="Save"
```

## Exit Codes

- `0`: Matching accessible element was found.
- `1`: Browser is not running or the assertion failed.
