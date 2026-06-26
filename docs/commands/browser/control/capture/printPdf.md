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

## Stdout

```text
PASS 001 printPdf path=page.pdf
PDF 001 C:\path\page.pdf
```

## Exit Codes

- `0`: PDF was written.
- `1`: Browser is not running or the action failed.
