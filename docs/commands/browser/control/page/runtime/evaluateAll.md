# `browser control page runtime evaluateAll`

Evaluates JavaScript with all matching elements.

```powershell
cmg browser control page runtime evaluateAll "<selector>" "<expression>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<expression>`: JavaScript expression or function. Matching elements are available as `elements`.

## Stdout

```text
PASS 001 evaluateAll .item elements.length
EVALUATE 001 3
```

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running or the action failed.
