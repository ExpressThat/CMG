# `browser control context setGeolocation`

Sets page-visible geolocation.

```powershell
cmg browser control context setGeolocation <latitude> <longitude> [--accuracy <meters>]
```

## Arguments

- `<latitude>`: Latitude.
- `<longitude>`: Longitude.

## Options

- `--accuracy <meters>`: Coordinate accuracy. Default is `1`.

## Stdout

```text
PASS 001 setGeolocation latitude=51.5 longitude=-0.1 accuracy=10
GEOLOCATION 001 51.5,-0.1 accuracy=10
```

## Exit Codes

- `0`: Geolocation was set.
- `1`: Browser is not running or the action failed.
