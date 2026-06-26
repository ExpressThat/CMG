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

Blocks use `{` at the end of the parent line and `}` to close the block. Branch continuations can be written on the next line or combined with the closing brace; spacing and casing are tolerant for `elseif`, `else`, `catch`, and `finally`.

```text
if false {
  caption "if"
}   ELSE   {
  caption "else"
}

try {
  caption "try"
}catch error{
  caption "${error}"
} finally {
  caption "done"
}
```

Blocks can also be written inline. Braces inside quoted strings are preserved:

```text
describe "inline" { before { setContent "<main>{ready}</main>" } it "case" { if true { caption "yes" } else { caption "no" } } }
```

Semicolons can separate actions on the same line outside quoted strings:

```text
caption "one;still one"; if true { caption "two"; caption "three" }; caption "four"
```

Supported structural blocks:

- `suite "name" { ... }`, `describe "name" { ... }`, or `context "name" { ... }`
- `test "name" { ... }`, `it "name" { ... }`, or `specify "name" { ... }`
- `beforeAll { ... }` or `before { ... }`
- `afterAll { ... }` or `after { ... }`
- `beforeEach { ... }`
- `afterEach { ... }`
- `step "caption" { ... }`
- `gif "name" { ... }`
- `recordVideo "name" { ... }`
- `screencast "name" { ... }`
- `within "<containerSelector>" { ... }`
- `if <condition> { ... }`
- `elseif <condition> { ... }`
- `else { ... }`
- `switch <value> { case <value> { ... } default { ... } }`
- `for <count> { ... }`
- `for <variable> <start> <end> { ... }`
- `repeat <count> { ... }`
- `repeat <variable> <count> { ... }`
- `while <condition> max=100 { ... }`
- `until <condition> max=100 { ... }`
- `doWhile <condition> max=100 { ... }`
- `doUntil <condition> max=100 { ... }`
- `retry [count|max=<count>] delay=<milliseconds> { ... }`
- `toPass [count|max=<count>] delay=<milliseconds> { ... }`
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

Indentation, tabs, and repeated spaces between tokens are ignored outside quoted strings:

```text
          click          "#save"          timeout=5000
		caption          "Done"
```

Spaces and semicolons inside quoted strings are preserved.

## Imports

Use a line-level import to include reusable macros or setup actions:

```text
import "shared.cmgscript"
```

Imports are expanded before parsing. Relative paths resolve from the importing file's directory. Imported files can import other files; cycles and missing files fail before any action runs.

`import` is case-insensitive and tolerates any whitespace before the quoted path, including tabs:

```text
  IMPORT    "shared.cmgscript"
import	"shared.cmgscript"
```

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

Conditions support static values, variables, `==`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `matches`, `in`, `&&`, `||`, unary `!`, strings, numbers, and empty strings:

```text
if (${count} > 5 && !(${mode} == "")) {
  click "#save"
} elseif (${mode} == "preview") {
  hover "#save"
} elseif (${mode} in "checkout" "billing") {
  caption "Payment flow"
} elseif (evaluate "window.checkoutReady" == "true") {
  caption "Browser state is ready"
} else {
  caption "Nothing to save"
}
```

Conditions can also run actions. Actions that emit a payload, such as `evaluate`, `title`, element getters, file reads, and `call`, can be compared with the same operators. Actions that do not emit a payload are treated as true when they succeed and false when they fail:

```text
if (assertText "#status" "Saved") {
  click "#continue"
}
```

Use `switch` when a value has several branches. `case` defaults to equality and also supports `==`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `matches`, and `in`:

```text
switch ${mode} {
  case "profile" {
    caption "Profile flow"
  }
  case in "checkout" "billing" {
    caption "Payment flow"
  }
  default {
    caption "Fallback flow"
  }
}
```

The `switch` subject can also be a value-producing action:

```text
switch title {
  case contains "Checkout" {
    caption "Checkout page"
  }
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

within ".dialog" {
  fill "input[name=email]" "agent@example.com"
  click "button.save"
}
```

`repeat`, `for`, `foreach`, `foreachSelector`, `while`, `until`, `doWhile`, and `doUntil` support `break` and `continue`. Condition loops have a safety guard and fail after `max=<count>` iterations; the default is `100`. `doWhile` and `doUntil` run their body once before evaluating the condition.

`foreachSelector` binds the variable to a temporary CSS selector for each matched element and also exposes `${index}`. Macro definitions are block-scoped when declared inside another macro, branch, or loop. Top-level macros in `cmg run` are registered before each test.

`within "<container>" { ... }` scopes selector-based child actions to the container. Nested `within` blocks compose selectors. Pointer-aware child actions still use CMG's virtual pointer and GIF event path after scoping.

Use `retry` or provider-style `toPass` to rerun a block until it succeeds or the attempt limit is exhausted:

```text
retry max=3 delay=100 {
  click "#save"
  assertText "#status" "Saved"
}

toPass max=3 delay=100 {
  assertText "#status" "Saved"
}
```

`retry 3 { ... }` and `toPass 3 { ... }` are positional forms. `max=` must be greater than `0`; `delay=` is an optional pause in milliseconds between failed attempts.

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
