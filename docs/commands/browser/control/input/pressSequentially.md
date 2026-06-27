# `browser control input pressSequentially`

Runs the scripting `pressSequentially` action once from the command line.

```powershell
cmg browser control input pressSequentially "<selector>" "<text>" [--delay <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<text>`: Text to enter.

## Options

- `--delay <milliseconds>`: Optional delay between typed characters. Defaults to `80` milliseconds in GIF recordings and the fast native type path outside GIF recording when omitted. Must be zero or greater.

## Stdout

```text
PASS 001 pressSequentially #name CMG
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Text was typed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input pressSequentially "#name" "CMG"
cmg browser control input pressSequentially "#name" "CMG" --delay 25
```
