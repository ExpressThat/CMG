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
```
