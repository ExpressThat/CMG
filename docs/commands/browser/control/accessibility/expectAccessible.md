# `browser control accessibility expectAccessible`

Runs the scripting `expectAccessible` action once from the command line.

```powershell
cmg browser control accessibility expectAccessible --role <role> [--name <text>]
```

## Options

- `--role <role>`: Required accessible role.
- `--name <text>`: Optional text expected in the accessible name.

## Stdout

```text
PASS 001 expectAccessible role=button name=Save
ACCESSIBLE 001 button Save
```

## Stderr

Writes browser, accessibility, parse, or action errors.

## Exit Codes

- `0`: Matching accessible element exists.
- `1`: Browser is not running, the command is invalid, or no match exists.

## Examples

```powershell
cmg browser control accessibility expectAccessible --role button --name Save
```
