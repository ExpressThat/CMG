# `browser control context javaScriptEnabled`

Runs the scripting `javaScriptEnabled` action once from the command line.

```powershell
cmg browser control context javaScriptEnabled <enabled>
```

## Arguments

- `<enabled>`: `true` to enable dynamic JavaScript execution, `false` to disable it for future page scripts through the CMG shim.

## Stdout

```text
PASS 001 javaScriptEnabled true
JAVASCRIPT_ENABLED 001 true
```

## Stderr

Writes browser or boolean parsing errors.

## Exit Codes

- `0`: JavaScript behavior was configured.
- `1`: Browser is not running, the value was invalid, or the action failed.

## Example

```powershell
cmg browser control context javaScriptEnabled false
```
