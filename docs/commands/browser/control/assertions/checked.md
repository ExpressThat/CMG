# `browser control assertions checked`

Runs the scripting `expectChecked` action once from the command line.

```powershell
cmg browser control assertions checked "<selector>" [--expected <true|false>] [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--expected <true|false>`: Expected checked state. Defaults to `true`.
- `--timeout <ms>`: Poll until the checked state matches or the timeout expires.

## Stdout

```text
PASS 001 expectChecked #agree
EXPECT 001 checked #agree
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element checked state matched.
- `1`: Browser is not running, no element matched, state did not match, or the timeout expired.

## Examples

```powershell
cmg browser control assertions checked "#agree"
cmg browser control assertions checked "#agree" --expected false
```
