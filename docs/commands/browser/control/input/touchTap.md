# `browser control input touchTap`

Runs the scripting `touchTap` action once from the command line.

```powershell
cmg browser control input touchTap "<selector>"
cmg browser control input touchTap --x 120 --y 240
```

## Arguments

- `<selector>`: Optional CSS selector or supported rich locator. Omit it when using `--x` and `--y`.

## Options

- `--x <pixels>`: Viewport X coordinate to tap. Must be used with `--y` when no selector is provided.
- `--y <pixels>`: Viewport Y coordinate to tap. Must be used with `--x` when no selector is provided.

## Stdout

```text
PASS 001 touchTap #save
TAP 001 #save
PASS 001 touchTap x=120 y=240
TAP 001 120,240
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Tap completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input touchTap "#save"
cmg browser control input touchTap --x 120 --y 240
```

## GIF Recording

When used through `browser control script --gif` or `cmg run --gif`, selector taps move the virtual pointer to the element and coordinate taps move it to the same viewport point before the touch-style event sequence and click pulse are recorded.
