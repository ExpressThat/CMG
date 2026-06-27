# `browser control storage state`

Runs the scripting `storageState` action once from the command line.

```powershell
cmg browser control storage state <operation> [--path <path>]
```

## Arguments

- `<operation>`: `save` or `load`.

## Options

- `--path <path>`: Storage state JSON file path. Defaults to `cmg-storage-state.json`.

## Stdout

```text
PASS 001 storageState save
STORAGE_STATE 001 saved C:\Projects\CMG\artifacts\auth.json
```

For load:

```text
PASS 001 storageState load
STORAGE_STATE 001 loaded C:\Projects\CMG\artifacts\auth.json
```

## Stderr

Writes browser, file, JSON, or argument errors.

## Exit Codes

- `0`: The storage state operation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control storage state save --path artifacts\auth.json
```
