# `.cmgscript` Syntax

CMG scripts are structured test files for `cmg run`. Direct `browser control script` files use the same action syntax and can also use action blocks such as `dragAndDrop` and `gif`.

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
- `recordVideo "name" { ... }`
- `screencast "name" { ... }`

Tests can include options:

```text
test "checkout" tag=smoke,critical {
  click "#checkout"
}
```

Use `cmg run --tag smoke` to run tests with a matching tag. Use comma-separated tags when a test belongs to multiple groups.

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

`gif "name" { ... }`, `recordVideo "name" { ... }`, and `screencast "name" { ... }` record only the wrapped actions when direct `browser control script` or `cmg run` is used without command-level `--gif`.

When command-level `--gif` is used, CMG records the entire script or test and suppresses nested block recordings. Actions inside `gif` blocks still run and are included in the whole-run GIF.
