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
- Provider-style declaration aliases: `test.only`, `test.skip`, `test.fixme`, `test.todo`, `test.slow`, `it.only`, `describe.skip`, `describe.slow`, `context.only`, and the same suffixes on `suite`, `describe`, `context`, `test`, `it`, and `specify`
- Parameterized runner test declarations: `test.each`, `it.each`, and `specify.each`
- `beforeAll { ... }` or `before { ... }`
- `afterAll { ... }` or `after { ... }`
- `beforeEach { ... }`
- `afterEach { ... }`
- `step "caption" { ... }` in direct scripts and `cmg run`
- `gif "name" { ... }`
- `recordVideo "name" { ... }`
- `screencast "name" { ... }`
- `within "<containerSelector>" { ... }`
- `frame "<iframeSelector>" { ... }` or `frameLocator "<iframeSelector>" { ... }`
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
- `withTimeout <milliseconds> { ... }`
- `withTimeout default=<milliseconds> navigation=<milliseconds> assertion=<milliseconds> { ... }`
- `withDefaultTimeout <milliseconds> { ... }`
- `withNavigationTimeout <milliseconds> { ... }`
- `withAssertionTimeout <milliseconds> { ... }` or `withExpectTimeout <milliseconds> { ... }`
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

test "checkout with metadata" owner=qa issue="BUG-7" link="https://example.test/BUG-7" annotation.requirement="REQ-1" {
  click "#checkout"
}
```

Use `cmg run --tag smoke` to run tests with a matching tag. Use comma-separated tags when a test belongs to multiple groups.
Report metadata options do not affect selection or browser behavior. `owner=`, `issue=`, `link=`, `requirement=`, `note=`, and `annotation.<type>=...` are written to JSON, HTML, and JUnit reports. Suite metadata cascades to child tests.

Runner declarations can also provide initial scoped variables with `var.<name>=<value>`. Suite variables cascade to child tests, and test-level variables override suite values:

```text
describe "tenant" var.tenant=demo {
  test "default tenant" {
    expect (${tenant} == "demo")
  }

  test "staging tenant" var.tenant=staging {
    expect (${tenant} == "staging")
  }
}
```

Runner declarations can provide `baseUrl=` or `baseURL=` for relative navigation. Suite values cascade to child tests, and test-level values override suite and command-line `--base-url` values:

```text
describe "tenant" baseUrl="https://example.test/app/" {
  test "opens profile" {
    navigate "profile"
  }
}
```

`only`, `skip`, `fixme`, `todo`, and `slow` can be written either as options or provider-style dotted declarations. Dotted declarations normalize before planning:

```text
test.only "focused" { click "#run" }
test.skip "legacy" reason="Disabled" { click "#legacy" }
test.fixme "broken checkout"
test.slow "long visual flow" { click "#save" }
describe.skip "legacy area" { it "old case" { click "#old" } }
describe.slow "slow area" { it "eventual case" { expectText "#status" "Saved" } }
```

Suite-level `only`, `skip`, `fixme`, and `todo` options cascade to child tests. A skipped suite keeps child tests skipped even if a child sets `skip=false`.
Suite-level `slow` also cascades unless a child test sets `slow=false`. `slow=true` uses a `3x` multiplier for inherited default wait, navigation, and assertion timeouts; `slow=<number>` uses that numeric multiplier. Explicit action-level `timeout=` still wins.

Runner declarations can also define command-level GIF defaults:

```text
describe "mobile evidence" gifQuality=high gifPointerSpeed=fast gifFps=20 gifViewport=390x844 gifPixelRatio=2 {
  test "inherits suite defaults" { click "#save" }
  test "uses a smaller artifact" gifQuality=medium gifScale=0.75 { click "#save" }
}
```

Supported keys are `gifQuality`, `gifPointerDuration`, `gifPointerSpeed`, `gifPointerEasing`, `gifFps`, `gifFrameDelay`, `gifCrop`, `gifCropPadding`, `gifScale`, `gifMaxWidth`, `gifMaxHeight`, `gifViewport`, and `gifPixelRatio`. Suite values cascade and tests override property by property. Invalid values fail that test with the exact declaration option.

Runner declarations also support artifact retention:

```text
describe "CI evidence" gif=onFailure gifSampleRate=5 {
  test "keeps failed attempts" gif=onRetry { click "#save" }
  test "report then delete" gif=always gifCleanPassed=true { click "#publish" }
}
```

`gif=` accepts `always`, `onFailure`, `onRetry`, or `off`. `gifSampleRate=<n>` deterministically records the first selected test and every nth test after it. `gifCleanPassed=true` deletes passing command-level artifacts after reports and traces are written. The declarations cascade through suites and affect only whole-test capture requested with `cmg run -gif`; explicit focused recording blocks remain independent.

Parameterized runner tests expand one declaration into one scheduled test per row:

```text
test.each "opens ${page}" as=page values="profile,checkout" tag=smoke {
  click "#${page}"
}

test.each "opens ${case.name}" as=case json="[{\"name\":\"Profile\",\"selector\":\"#profile\"}]" {
  click "${case.selector}"
}
```

Use `values=` or `each=` for delimited primitive rows. Use `json=` for a JSON array. Primitive rows bind `${item}` by default, or the variable named by `as=`.
JSON object rows bind the raw object to `${item}` and each top-level property to dotted variables such as `${item.name}`. `${index}` is also available. Expanded tests participate in grep, tags, focus, skip, retries, sharding, reports, traces, and per-test GIFs as ordinary tests.

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

Locator option tokens can use CMG locator names or provider-style aliases such as `getByText=Save`, `getByRole=button|Save`, `getByLabel=Email`, `getByTestId=save`, `getByPlaceholder=Search`, `getByAltText=Logo`, and `getByTitle=Close`. Quote the whole locator, for example `"getByLabel=Profile name"`, when the value contains spaces. They work in direct scripts and structured `cmg run` tests, including inside nested blocks, macros, and GIF blocks.

Spaces, semicolons, braces, and `#` characters inside quoted strings are preserved.

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

Imports also participate in normal line splitting, so dense AI-authored files can put an import before another action on the same physical line:

```text
import "shared.cmgscript"; test "uses import" { call helper }
```

## Variables And Capture

Use `set` with a literal value or a block:

```text
set title {
  evaluate "document.title"
}
```

Block capture stores only the final payload value from the wrapped actions. It does not store the `PASS`, `EVALUATE`, source-line, context, or action metadata. This also works with `call`, so `set result { call helper }` stores the macro body's final payload value. A macro can return an action result by making that action the final payload, or it can return a variable/static value with `return "${value}"`.

Variables are referenced as `${name}`. Dotted names such as `${case.name}` are supported for data-driven runner rows and can also be set manually. A macro reads from its own parameters and local `set` values first, then walks upward through the parent tree scopes where that macro was defined until it finds a matching variable. It does not read unrelated local variables from a caller outside that definition tree. Macro parameters and every `set` performed inside a macro are scoped to that macro call and do not mutate variables with the same name in a parent scope. Loop variables are scoped to the loop iteration. Explicit `set` variables outside macros remain available to later actions.

Direct scripts and runner tests can receive initial variables from the command line with `--var name=value` or `--env name=value`. Runner tests can receive declaration variables through `var.<name>=<value>`. Command-line variables are applied before declaration variables, and explicit `set` actions can still replace values later in the current script scope.

## Control Flow And Macros

Conditions support static values, variables, `==`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `matches`, `in`, `&&`, `||`, unary `!`, strings, numbers, booleans, and empty strings:

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

expect (${count} > 5)
assert evaluate "document.title" contains "CMG"
softExpect (${count} > 10) message="Count was lower than expected"
```

Browser `evaluate` boolean payloads are normalized to lowercase `true` and `false`. Condition comparisons also accept boolean literals case-insensitively, so a captured `True` value still compares equal to `true`.

Use `expect` or `assert` when a condition should fail the current script or test instead of selecting a branch. Use `softExpect`, `softAssert`, `expect.soft`, or `assert.soft` when a failed condition should be reported after later actions run.

Use `skip "reason"` inside a branch or macro when runtime state makes the rest of a direct script or test not applicable:

```text
if (${featureEnabled} == false) {
  skip "Feature flag disabled"
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

frame "#checkoutFrame" {
  fill "#email" "agent@example.com"
  click "#save"
}
```

`repeat`, `for`, `foreach`, `foreachJson`, `foreachList`, `foreachSelector`, `while`, `until`, `doWhile`, and `doUntil` support `break` and `continue`. Condition loops have a safety guard and fail after `max=<count>` iterations; the default is `100`. `doWhile` and `doUntil` run their body once before evaluating the condition.

`foreachJson` iterates a JSON array, `foreachList` iterates a delimited string, and `foreachSelector` binds the variable to a temporary CSS selector for each matched element. All three expose `${index}`. Macro definitions are block-scoped when declared inside another macro, branch, or loop. Top-level macros in `cmg run` are registered before each test.

Script stdout uses one global sequence for the whole run. Actions inside macros, loops, `step`, retry, or handled branches include source context, for example:

```text
PASS 001 line=3 action=navigate https://example.test
PASS 007 line=42 context="macro login > repeat[2/3]" action=click #save
EVALUATE 008 line=43 context="macro login > repeat[2/3]" action=evaluate true
```

The `context=` field is omitted when there is no parent context. `PASS` and payload lines use a global run sequence so nested macro/loop output stays ordered. JSON and HTML runner reports expose sequence, source line, context, and action as separate fields for every public executed step. Generated internal evaluate/actionability/locator steps are omitted from public reports and kept in trace/debug artifacts.

`within "<container>" { ... }` scopes selector-based child actions to the container. Nested `within` blocks compose selectors. Pointer-aware child actions still use CMG's virtual pointer and GIF event path after scoping.

`frame "<iframe>" { ... }` and `frameLocator "<iframe>" { ... }` scope supported child actions to a same-origin iframe. Pointer-aware child actions are rewritten to frame-aware actions before GIF recording, so the virtual pointer moves to the element's top-page coordinate inside the iframe.

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

Use scoped timeout blocks when one part of a script needs different defaults without changing the rest of the run:

```text
withTimeout 10000 {
  waitForSelector "#slow-panel"
}

withTimeout default=5000 navigation=15000 assertion=2000 {
  navigate "https://example.com" waitUntil=load
  expectText "#status" "Ready"
}
```

Scoped timeout blocks can be nested inside macros, loops, `try`/`catch`, `within`, frame blocks, and GIF blocks. Previous timeout defaults are restored after the block, even when a child action fails and is caught by a surrounding `try`.

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

Blank lines and comments are ignored. A `#` starts a comment when it is outside a quoted string or variable reference and appears at the start of a line, or when it appears after whitespace and is followed by whitespace or the end of the line. CSS id selectors such as `#save` remain valid in unquoted arguments.

```text
# Open the local test page
navigate "C:\Projects\CMG\index.html"
click "#save" # submit the form
click #save # unquoted CSS id selector
caption "Saved #1"
```

## GIF Blocks

`gif "name" { ... }`, `recordVideo "name" { ... }`, and `screencast "name" { ... }` record only the wrapped actions when direct `browser control script` or `cmg run` is used without command-level `--gif`.

When command-level `--gif` is used, CMG records the entire script or test and suppresses nested block recordings. Actions inside `gif` blocks still run and are included in the whole-run GIF.
