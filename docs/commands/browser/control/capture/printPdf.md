# `browser control capture printPdf`

Prints the current page to a PDF file.

```powershell
cmg browser control capture printPdf --path <file> [options]
```

## Options

- `--path <file>`: Required output PDF path.
- `--landscape <true|false>`: Use landscape orientation. Default is `false`.
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
PASS 001 printPdf path=page.pdf
PDF 001 C:\path\page.pdf
```

## Exit Codes

- `0`: PDF was written.
- `1`: Browser is not running, options are invalid, or the action failed.

## Examples

```powershell
cmg browser control capture printPdf --path page.pdf
cmg browser control capture printPdf --path page.pdf --format A4 --margin-top 10mm --margin-bottom 10mm
cmg browser control capture printPdf --path page.pdf --width 8.5in --height 11in --page-ranges 1-2,4
```
