# `browser control frames evaluate`

Evaluates JavaScript inside a same-origin iframe.

```powershell
cmg browser control frames evaluate "<frameSelector>" "<expression>"
```

## Arguments

- `<expression>`: JavaScript expression evaluated in the iframe.

## Stdout

```text
PASS 001 frameEvaluate #frame document.title
FRAME_EVALUATE 001 Checkout
```

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running or the action failed.
