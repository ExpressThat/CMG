# `browser control input scrollIntoView`

Runs the scripting `scrollIntoView` action once from the command line.

```powershell
cmg browser control input scrollIntoView "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 scrollIntoView #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was scrolled into view.
- `1`: Browser is not running or the action failed.
