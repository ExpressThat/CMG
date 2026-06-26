# `browser control capture pdf`

Runs the scripting `pdf` action once from the command line.

```powershell
cmg browser control capture pdf --path <file> [options]
```

## Options

- `--path <file>`: Required output PDF path.
- `--landscape <true|false>`: Use landscape orientation.
- `--print-background <true|false>`: Print backgrounds. Default is `true`.
- `--scale <number>`: Positive print scale. Default is `1`.
- `--format <name>`: Paper format: `Letter`, `Legal`, `Tabloid`, `Ledger`, or `A0` through `A6`.
- `--width <size>`: Custom paper width. Sizes accept bare inches, `in`, `cm`, `mm`, or `px`.
- `--height <size>`: Custom paper height.
- `--margin-top <size>`: Top page margin.
- `--margin-right <size>`: Right page margin.
- `--margin-bottom <size>`: Bottom page margin.
- `--margin-left <size>`: Left page margin.
- `--page-ranges <ranges>`: Pages to print, for example `1-3,5`.
- `--prefer-css-page-size`: Prefer the page's CSS `@page` size when the browser supports it.

## Stdout

```text
PASS 001 pdf path=page.pdf
PDF 001 page.pdf
```

## Stderr

Writes browser, file, PDF, parse, or action errors.

## Exit Codes

- `0`: PDF was written.
- `1`: Browser is not running, options are invalid, or PDF generation failed.

## Examples

```powershell
cmg browser control capture pdf --path page.pdf
cmg browser control capture pdf --path page.pdf --landscape true --print-background true
cmg browser control capture pdf --path page.pdf --format A4 --margin-top 10mm --page-ranges 1
```
