# `.cmgscript` Syntax

CMG scripts are structured test files.

```text
suite "profile" {
  beforeEach {
    navigate "https://example.com"
  }

  test "opens dialog" {
    step "Open dialog" {
      click "#openProfileDialog"
    }
  }
}
```

## Blocks

Blocks use `{` at the end of the parent line and `}` on its own line.

Supported structural blocks:

- `suite "name" { ... }`
- `test "name" { ... }`
- `beforeEach { ... }`
- `afterEach { ... }`
- `step "caption" { ... }`
- `gif "name" { ... }`

## Actions

Actions use:

```text
action positionalArgs... key=value...
```

Options use identifier-like keys. Selector values containing `=` remain positional arguments unless the key before `=` is identifier-like.

## Quoting

Use quotes for values that contain spaces or shell-sensitive characters.

```text
click "#openProfileDialog"
fill "#profileName" "CMG Test Profile"
```

Escapes inside quoted strings support `\"`, `\\`, `\n`, `\r`, and `\t`.

## Comments

Blank lines and full-line comments are ignored.

```text
# Open the local test page
navigate "C:\Projects\CMG\index.html"
```

## GIF Blocks

`gif "name" { ... }` records only the wrapped actions when `cmg run` is used without command-level `--gif`.

When `cmg run --gif <directory>` is used, CMG records the entire test and suppresses nested block recordings. Actions inside `gif` blocks still run and are included in the whole-test GIF.
