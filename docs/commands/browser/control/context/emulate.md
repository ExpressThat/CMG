# `browser control context emulate`

Applies page environment and viewport emulation.

```powershell
cmg browser control context emulate [options]
```

## Options

- `--width <pixels>`: Viewport width in CSS pixels. Use with `--height`.
- `--height <pixels>`: Viewport height in CSS pixels. Use with `--width`.
- `--device <name>`: Named device preset. Supported names are `iPhone 13`, `iPhone SE`, `Pixel 5`, `Pixel 7`, `Galaxy S9+`, `iPad`, `iPad Pro`, and `Desktop Chrome`.
- `--device-scale-factor <number>`: Viewport device scale factor.
- `--mobile`: Use mobile viewport metrics.
- `--touch`: Enable touch viewport hints.
- `--user-agent <text>`: Page-visible `navigator.userAgent` value.
- `--locale <locale>`: Page-visible locale, such as `en-GB`.
- `--timezone <zone>`: Reported IANA timezone.
- `--color-scheme <scheme>`: Preferred color scheme, such as `light` or `dark`.
- `--reduced-motion <value>`: Reduced motion value, such as `reduce` or `no-preference`.
- `--geolocation <lat,lng>`: Stubbed coordinates.
- `--permissions <names>`: Comma-separated granted permission names.

## Stdout

```text
PASS 001 emulate width=390 height=844 isMobile=true hasTouch=true
EMULATE 001 width height isMobile hasTouch
```

## Stderr

Writes browser, option, or action errors. For example, width without height fails.
Unknown device names fail and list the supported names.

## Exit Codes

- `0`: Emulation was applied.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control context emulate --device "Pixel 7"
cmg browser control context emulate --device "iPhone 13" --locale en-GB --timezone Europe/London
cmg browser control context emulate --device "iPad Pro" --width 1024 --height 900
```
