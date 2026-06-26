# `browser control input selectOption`

Runs the scripting `selectOption` action once from the command line.

```powershell
cmg browser control input selectOption "<selector>" "<value>"
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<value>`: Option value to select.

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
```
