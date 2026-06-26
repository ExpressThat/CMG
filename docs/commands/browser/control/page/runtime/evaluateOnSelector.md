# `browser control page runtime evaluateOnSelector`

Evaluates JavaScript with one selected element.

```powershell
cmg browser control page runtime evaluateOnSelector "<selector>" "<expression>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<expression>`: JavaScript expression or function. The selected element is available as `element`.

## Stdout

```text
PASS 001 evaluateOnSelector h1 element.textContent
EVALUATE 001 Heading
```

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running or the action failed.
