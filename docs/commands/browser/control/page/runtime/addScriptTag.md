# `browser control page runtime addScriptTag`

Injects a script tag into the current page.

```powershell
cmg browser control page runtime addScriptTag ["<content>"] [--url <url>] [--path <file>] [--content <source>]
```

## Inputs

- `[content]`: Inline script content.
- `--url <url>`: Script URL.
- `--path <file>`: Local script file whose content is injected.
- `--content <source>`: Inline content option.

## Stdout

```text
PASS 001 addScriptTag window.__tag = true;
SCRIPT_TAG 001 content
```

## Exit Codes

- `0`: Script tag was injected.
- `1`: Browser is not running or the action failed.
