# CMG

CMG is a browser automation CLI for producing choreographed, human-readable, pointer-accurate visual evidence.

It drives a real browser from scripts, test files, or one-shot CLI commands. When you ask it to record a GIF, CMG shows the journey with a live virtual pointer, real pointer events, hover states, drag ghosts, captions, and partial recordings when something fails. The result is automation evidence that people can understand quickly and agents can parse reliably.

## Why CMG

Most browser automation tools prove that something happened. CMG is designed to show how it happened.

- **Pointer-accurate GIFs**: clicks, hovers, typing, scrolls, drags, offsets, captions, and drag ghosts are recorded through CMG's virtual pointer path.
- **Visual evidence by default when you ask for it**: use `--gif` for a whole run, or a `gif { ... }` block for a focused moment.
- **Readable scripts**: `.cmgscript` is intentionally compact, whitespace-tolerant, and friendly to AI-generated browser control.
- **Tests and browser control**: use `cmg run` for structured tests, reports, retries, traces, and sharding; use `browser control script` when an agent needs to control the browser directly.
- **Useful failures**: failed steps include the action, line number, and reason in stderr, reports, and traces.
- **Broad automation without hiding the story**: browser control, assertions, locators, frames, network, storage, files, screenshots, accessibility, and visual checks are covered in the [action index](docs/scripting/action-index.md), but recorded runs still look like a guided browser journey.

CMG is not trying to be a clone of Cypress, Puppeteer, or Playwright. It gives you the automation coverage you expect, but its center of gravity is different: CMG creates inspectable visual evidence that reads like a guided browser performance.

## When To Use CMG

| Job | CMG fit |
| --- | --- |
| Make a PR demo GIF | Strong fit: record the actual browser journey with pointer movement, captions, hovers, and drag behavior. |
| Reproduce a visual bug | Strong fit: capture the steps and the failure as a shareable artifact. |
| Let an AI inspect or control a browser | Strong fit: scripts are compact, output is parseable, and failures explain what broke. |
| Run repeatable smoke tests with reports | Strong fit: use `cmg run` with reports, traces, retries, sharding, and optional per-test GIFs. |
| Maintain a very large CI regression suite | Possible, but Playwright or Cypress may still be the default choice when ecosystem maturity matters more than visual evidence. |

The useful difference is not "CMG has aliases like other tools." The useful difference is that CMG turns automation into evidence that a human can watch and an agent can reason about.

## Install And Run Locally

From this repository:

```powershell
dotnet build /p:UseSharedCompilation=false
dotnet run -- browser launch
```

Published command examples use `cmg`:

```powershell
cmg browser launch
```

While developing in this repo, replace `cmg` with `dotnet run --`.

## 60-Second Happy Path

```powershell
cmg browser launch
cmg browser control script --file demo.cmgscript --gif demo.gif
cmg run tests --report-html report.html
```

That is the shape of CMG: launch a controlled browser, drive it with readable scripts, record visual proof when useful, and run structured tests when you need reports.

For the full three-path walkthrough, see the [Quick Start](docs/quick-start.md).

## Make Your First GIF

Create `first-gif.cmgscript`:

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
step "Open the profile dialog" {
  click "#openProfileDialog"
}
type "#profileName" "My first CMG GIF"
```

Run it with GIF recording:

```powershell
cmg browser launch
cmg browser control script --file first-gif.cmgscript --gif demo-output\first-gif.gif
```

The GIF shows the visible browser viewport, the virtual pointer moving to the target, pointer events firing against the page, typed text appearing progressively, and the step caption.

## Write Your First Test

Create `first-test.cmgscript`:

```text
suite "profile dialog" {
  beforeEach {
    navigate "C:\Projects\CMG\index.html"
    waitForElement "#openProfileDialog" timeout=5000
  }

  test "opens and captures the dialog" tag=smoke {
    step "Open dialog" {
      click "#openProfileDialog"
    }

    expectVisible "#profileDialog"
    expectText "#lastDialogAction" "None"

    gif "dialog evidence" {
      type "#profileName" "CMG Test Profile"
      screenshot "#profileDialog" output="demo-output\profile-dialog.png"
    }
  }
}
```

Run the test with reports and whole-test GIFs:

```powershell
cmg run first-test.cmgscript --gif demo-output\gifs --report-html demo-output\report.html --report-json demo-output\report.json
```

When `--gif` is used, CMG records the entire selected test and suppresses nested `gif` block files. The nested actions still run and appear in the whole-test GIF.

## Script Or Test?

| Use case | Command | Best for |
| --- | --- | --- |
| Direct browser control | `cmg browser control script --file flow.cmgscript --gif flow.gif` | Agent-controlled exploration, demos, bug reproduction, and short visual journeys. |
| Structured test runs | `cmg run tests --report-html report.html --gif gifs` | Repeatable checks, PR evidence, reports, retries, sharding, and per-test traces. |
| One-shot CLI actions | `cmg browser control input click "#save"` | Simple agent operations without writing a script file. |

Both script types use the same action syntax, control flow, macros, locators, GIF behavior, and virtual pointer model. The runner adds suites, hooks, selection, retries, sharding, reports, and per-test traces. See [Script vs Runner](docs/scripting/script-vs-runner.md) for the longer comparison.

## Learn More

- [Docs overview](docs/README.md)
- [Quick Start](docs/quick-start.md)
- [Command reference](docs/commands.md)
- [Scripting guide](docs/scripting/index.md)
- [Script vs Runner](docs/scripting/script-vs-runner.md)
- [Action index](docs/scripting/action-index.md)
- [GIF recording](docs/scripting/gif-recording.md)
- [Script style guide](docs/scripting/style-guide.md)
- [Runnable demos](demo-scripts/README.md)

## Documentation For Agents

CMG is intended to be called by AI agents. Command docs include arguments, options, stdout, stderr, exit codes, and examples. Scripting docs include syntax, action behavior, error handling, migration notes, and style guidance.

When adding or changing a command or scripting feature, update `docs/` in the same change.
