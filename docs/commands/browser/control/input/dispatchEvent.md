# `browser control input dispatchEvent`

Dispatches an `Event` or `CustomEvent` on an element.

```powershell
cmg browser control input dispatchEvent "<selector>" "<event>" [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<event>`: DOM event name.

## Options

- `--detail <json>`: JSON detail payload for `CustomEvent`.
- `--bubbles <true|false>`: Whether the event bubbles. Default is `true`.
- `--cancelable <true|false>`: Whether the event is cancelable. Default is `true`.

## Stdout

```text
PASS 001 dispatchEvent #target ready
EVENT 001 ready #target
```

## Exit Codes

- `0`: Event was dispatched.
- `1`: Browser is not running or the action failed.
