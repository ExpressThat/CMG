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
- `beforeAll { ... }`
- `afterAll { ... }`
- `beforeEach { ... }`
- `afterEach { ... }`
- `step "caption" { ... }`
- `gif "name" { ... }`
- `recordVideo "name" { ... }`
- `screencast "name" { ... }`
- `if <condition> { ... }`
- `elseif <condition> { ... }`
- `else { ... }`
- `for <count> { ... }`
- `for <variable> <start> <end> { ... }`
- `repeat <count> { ... }`
- `repeat <variable> <count> { ... }`
- `while <condition> max=100 { ... }`
- `foreach <variable> <value>... { ... }`
- `foreachSelector <variable> "<selector>" { ... }`
- `try { ... }`
- `catch [errorVariable] { ... }`
- `finally { ... }`
- `macro <name> [parameter...] { ... }`

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

## Imports

Use a line-level import to include reusable macros or setup actions:

```text
import "shared.cmgscript"
```

Imports are expanded before parsing. Relative paths resolve from the importing file's directory. Imported files can import other files; cycles and missing files fail before any action runs.

## Variables And Capture

Use `set` with a literal value or a block:

```text
set title {
  evaluate "document.title"
}
```

Block capture stores only the final payload value from the wrapped actions. It does not store the `PASS`, `EVALUATE`, or other output prefixes. This also works with `call`, so `set result { call helper }` stores the macro body's final payload value. A macro can return an action result by making that action the final payload, or it can return a variable/static value with `return "${value}"`.

Variables are referenced as `${name}`. A macro reads from its own parameters and local `set` values first, then walks upward through the parent tree scopes where that macro was defined until it finds a matching variable. It does not read unrelated local variables from a caller outside that definition tree. Macro parameters and every `set` performed inside a macro are scoped to that macro call and do not mutate variables with the same name in a parent scope. Loop variables are scoped to the loop iteration. Explicit `set` variables outside macros remain available to later actions.

## Control Flow And Macros

Conditions support static values, variables, `==`, `!=`, `>`, `>=`, `<`, `<=`, `&&`, `||`, unary `!`, strings, numbers, and empty strings:

```text
if (${count} > 5 && !(${mode} == "")) {
  click "#save"
} elseif (${mode} == "preview") {
  hover "#save"
} else {
  caption "Nothing to save"
}
```

Conditions can also run assertion or wait actions:

```text
if (assertText "#status" "Saved") {
  click "#continue"
}
```

Loops and macros can be nested in any combination:

```text
macro choose item label {
  if (${label} != "") {
    click "${item}"
  }
}

foreachSelector row ".result" {
  call choose "${row}" "open"
}
```

`repeat`, `for`, `foreach`, `foreachSelector`, and `while` support `break` and `continue`. `while` has a safety guard and fails after `max=<count>` iterations; the default is `100`.

`foreachSelector` binds the variable to a temporary CSS selector for each matched element and also exposes `${index}`. Macro definitions are block-scoped when declared inside another macro, branch, or loop. Top-level macros in `cmg run` are registered before each test.

Recoverable failure blocks use `try`, optional `catch`, and optional `finally`:

```text
try {
  click "#maybe-there"
} catch error {
  caption "${error}"
} finally {
  screenshotPage output="demo-output\after-try.png"
}
```

`catch error` binds the failure message to `${error}` for the catch block. If there is no matching `catch`, the original failure is reported after `finally` runs.

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
