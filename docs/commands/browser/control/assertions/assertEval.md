# `browser control assertions assertEval`

Runs the scripting `assertEval` action once from the command line.

```powershell
cmg browser control assertions assertEval "<expression>" [--equals <value>] [--contains <text>] [--timeout <ms>]
```

## Arguments

- `<expression>`: JavaScript expression to evaluate in the active page.

## Options

- `--equals <value>`: Require the evaluated string value to exactly match `<value>`.
- `--contains <text>`: Require the evaluated string value to contain `<text>`.
- `--timeout <ms>`: Poll until the assertion passes or the timeout expires.

## Stdout

```text
PASS 001 assertEval document.title
EXPECT_EVAL 001 Checkout
```

## Stderr

Writes browser, JavaScript, timeout, parse, or assertion failure errors.

## Exit Codes

- `0`: Expression result matched, contained the text, or was truthy.
- `1`: Browser is not running, JavaScript failed, assertion failed, or the timeout expired.

## Examples

```powershell
cmg browser control assertions assertEval "document.title" --equals "Checkout"
```
