# `browser control page runtime evaluateOnNewDocument`

Runs the scripting `evaluateOnNewDocument` action once from the command line.

```powershell
cmg browser control page runtime evaluateOnNewDocument ["<source>"] [--path <file>]
```

## Arguments

- `[source]`: Inline JavaScript source.

## Options

- `--path <file>`: JavaScript file to register.

## Stdout

```text
PASS 001 evaluateOnNewDocument window.__ready = true;
INIT_SCRIPT 001 <id>
```

## Stderr

Writes browser, file, JavaScript, parse, or action errors.

## Exit Codes

- `0`: Init script was registered.
- `1`: Browser is not running, neither source nor path was provided, the file is missing, or the action failed.

## Examples

```powershell
cmg browser control page runtime evaluateOnNewDocument "window.__ready = true"
cmg browser control page runtime evaluateOnNewDocument --path setup.js
```
