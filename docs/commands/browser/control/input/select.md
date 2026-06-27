# `browser control input select`

Runs the scripting `select` action once from the command line.

```powershell
cmg browser control input select "<selector>" ["<value>"] [--value <value>] [--label <label>] [--index <index>]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<value>`: Optional option value to select.

## Options

- `--value <value>`: Option value to select. Use this when no positional value is supplied.
- `--label <label>`: Visible option label to select.
- `--index <index>`: Zero-based option index to select. Must be zero or greater.

## Stdout

```text
PASS 001 select #country GB
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The select-like element value was changed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input select "#country" "GB"
cmg browser control input select "#country" --label "United Kingdom"
cmg browser control input select "#country" --index 2
```
