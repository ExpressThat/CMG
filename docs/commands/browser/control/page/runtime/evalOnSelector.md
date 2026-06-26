# `browser control page runtime evalOnSelector`

Runs the scripting `evalOnSelector` action once from the command line.

```powershell
cmg browser control page runtime evalOnSelector "<selector>" "<expression>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<expression>`: JavaScript expression or function. The selected element is available as `element`.

## Stdout

```text
PASS 001 evalOnSelector h1 element.textContent
EVALUATE 001 Heading
```

## Stderr

Writes browser, selector, JavaScript, parse, or action errors.

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running, the selector is missing, or evaluation failed.

## Examples

```powershell
cmg browser control page runtime evalOnSelector "h1" "element.textContent"
```
