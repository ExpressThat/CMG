# `browser control page runtime addInitScript`

Registers JavaScript for future documents.

```powershell
cmg browser control page runtime addInitScript ["<source>"] [--path <file>]
```

## Arguments

- `[source]`: Inline JavaScript source.

## Options

- `--path <file>`: JavaScript file to register.

## Stdout

```text
PASS 001 addInitScript window.__ready = true;
INIT_SCRIPT 001 <id>
```

## Exit Codes

- `0`: Init script was registered.
- `1`: Browser is not running or the action failed.
