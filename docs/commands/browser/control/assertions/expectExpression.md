# `browser control assertions expectExpression`

Runs the scripting `expectExpression` action once from the command line.

```powershell
cmg browser control assertions expectExpression "<expression>" [--equals <value>] [--contains <text>] [--timeout <ms>]
```

## Arguments

- `<expression>`: JavaScript expression to evaluate in the active page.

## Options

- `--equals <value>`: Require the evaluated string value to exactly match `<value>`.
- `--contains <text>`: Require the evaluated string value to contain `<text>`.
- `--timeout <ms>`: Poll until the assertion passes or the timeout expires.

## Stdout

```text
PASS 001 expectExpression window.appReady
EXPECT_EVAL 001 true
```

## Stderr

Writes browser, JavaScript, timeout, parse, or assertion failure errors.

## Exit Codes

- `0`: Expression result matched, contained the text, or was truthy.
- `1`: Browser is not running, JavaScript failed, assertion failed, or the timeout expired.

## Examples

```powershell
cmg browser control assertions expectExpression "window.appReady" --timeout 5000
```
