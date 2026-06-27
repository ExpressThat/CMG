# `browser control context emulateMedia`

Runs the scripting `emulateMedia` action once from the command line.

```powershell
cmg browser control context emulateMedia [options]
```

## Options

- `--media <type>`: CSS media type. Accepts `screen` or `print`.
- `--color-scheme <scheme>`: Preferred color scheme. Accepts `light`, `dark`, or `no-preference`.
- `--reduced-motion <value>`: Reduced motion preference. Accepts `reduce` or `no-preference`.
- `--forced-colors <value>`: Forced colors preference. Accepts `active` or `none`.
- `--contrast <value>`: Preferred contrast. Accepts `more`, `less`, `custom`, or `no-preference`.

## Stdout

```text
PASS 001 emulateMedia media=print colorScheme=dark
MEDIA 001 media=print colorScheme=dark
```

## Stderr

Writes browser, option, or action errors. At least one option is required, and invalid option values report the accepted values.

## Exit Codes

- `0`: Media emulation was applied.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control context emulateMedia --media print
cmg browser control context emulateMedia --color-scheme dark --reduced-motion reduce
cmg browser control context emulateMedia --forced-colors active --contrast more
```
