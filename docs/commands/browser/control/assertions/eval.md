# `browser control assertions eval`

Runs the scripting `expectEval` action once from the command line.

```powershell
cmg browser control assertions eval "<expression>" [--equals <value>] [--contains <text>] [--timeout <ms>]
```

## Arguments

- `<expression>`: JavaScript expression to evaluate in the active page.

## Options

- `--equals <value>`: Require the evaluated string value to exactly match `<value>`.
- `--contains <text>`: Require the evaluated string value to contain `<text>`.
- `--timeout <ms>`: Poll until the assertion passes or the timeout expires.

## Stdout

```text
PASS 001 expectEval document.title
EXPECT_EVAL 001 Checkout
```

## Stderr

Writes browser, JavaScript, timeout, or assertion failure errors.

## Exit Codes

- `0`: Expression result matched, contained the text, or was truthy.
- `1`: Browser is not running, JavaScript failed, assertion failed, or the timeout expired.

## Examples

```powershell
cmg browser control assertions eval "document.title" --equals "Checkout"
cmg browser control assertions eval "window.appReady" --timeout 5000
```
