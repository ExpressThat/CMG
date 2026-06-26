# `browser control page runtime evalAll`

Runs the scripting `evalAll` action once from the command line.

```powershell
cmg browser control page runtime evalAll "<selector>" "<expression>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<expression>`: JavaScript expression or function. Matching elements are available to the generated evaluation.

## Stdout

```text
PASS 001 evalAll .row els => els.length
EVALUATE 001 3
```

## Stderr

Writes browser, selector, JavaScript, parse, or action errors.

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running, the selector is missing, or evaluation failed.

## Examples

```powershell
cmg browser control page runtime evalAll ".row" "els => els.length"
```
