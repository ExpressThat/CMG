# `browser control input tap`

Runs the scripting `tap` action once from the command line.

```powershell
cmg browser control input tap "<selector>"
cmg browser control input tap --x 120 --y 240
```

## Arguments

- `<selector>`: Optional CSS selector or CMG rich locator. Omit it when using `--x` and `--y`.

## Options

- `--x <pixels>`: Viewport X coordinate to tap. Must be used with `--y` when no selector is provided.
- `--y <pixels>`: Viewport Y coordinate to tap. Must be used with `--x` when no selector is provided.

## Stdout

```text
PASS 001 tap #touchTarget
TAP 001 #touchTarget
PASS 001 tap x=120 y=240
TAP 001 120,240
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Touch-style pointer events were dispatched.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input tap "#save"
cmg browser control input tap --x 120 --y 240
```

## GIF Recording

When used through `browser control script --gif` or `cmg run --gif`, selector taps move the virtual pointer to the element and coordinate taps move it to the same viewport point before the touch-style event sequence and click pulse are recorded.
