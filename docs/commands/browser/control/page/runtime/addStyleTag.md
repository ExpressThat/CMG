# `browser control page runtime addStyleTag`

Injects a style tag or stylesheet link into the current page.

```powershell
cmg browser control page runtime addStyleTag ["<content>"] [--url <url>] [--path <file>] [--content <css>]
```

## Inputs

- `[content]`: Inline CSS content.
- `--url <url>`: Stylesheet URL.
- `--path <file>`: Local CSS file whose content is injected.
- `--content <css>`: Inline CSS option.

## Stdout

```text
PASS 001 addStyleTag body { outline: 1px solid red; }
STYLE_TAG 001 content
```

## Exit Codes

- `0`: Style tag or link was injected.
- `1`: Browser is not running or the action failed.
