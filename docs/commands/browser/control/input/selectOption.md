# `browser control input selectOption`

Runs the scripting `selectOption` action once from the command line.

```powershell
cmg browser control input selectOption "<selector>" ["<value>"] [--value <value>] [--label <label>] [--index <index>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<value>`: Optional option value to select.

## Options

- `--value <value>`: Option value to select. Use this when no positional value is supplied.
- `--label <label>`: Visible option label to select.
- `--index <index>`: Zero-based option index to select. Must be zero or greater.

## Stdout

```text
PASS 001 selectOption #plan pro
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Option was selected.
- `1`: Browser is not running, no element matched, the value was invalid, or the action failed.

## Example

```powershell
cmg browser control input selectOption "#plan" "pro"
cmg browser control input selectOption "#plan" --label "Pro"
cmg browser control input selectOption "#plan" --index 2
```
