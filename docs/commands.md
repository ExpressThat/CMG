# CMG Command Reference

CMG is a CLI for controlling a browser and producing choreographed, human-readable, pointer-accurate visual evidence. It is intended to be called by AI agents, so command output should stay predictable and parseable.

New users should start with the [Quick Start](quick-start.md). Use this page when you need exact command groups, arguments, options, stdout, stderr, and exit codes.

## Development Invocation

From the repository root:

```powershell
dotnet run -- <command> [arguments] [options]
```

Published executable examples use `cmg`:

```powershell
cmg <command> [arguments] [options]
```

## Global Options

- `--chrome`: Use Chrome. This is optional because Chrome is the default. Put this before the command group, for example `cmg --chrome browser launch`.
- `--edge`: Use Microsoft Edge instead of the default Chrome browser. Put this before the command group, for example `cmg --edge browser launch`.
- `--firefox`: Use Firefox instead of the default Chrome browser. Put this before the command group, for example `cmg --firefox browser launch`.

Chrome is the default browser. `--chrome`, `--edge`, and `--firefox` are mutually exclusive. Chrome and Edge use Chrome DevTools Protocol. Firefox support uses WebDriver BiDi. Each browser keeps separate launch state and profile data.

## Browser Instance Selection

The `browser` command group accepts `--port <port>` after `browser` and before the leaf command:

```powershell
cmg browser --port 9333 launch --headless
cmg browser --port 9333 control page evaluate "document.title"
cmg browser --port 9333 close
```

This runs or controls a separate same-browser instance. Without `--port`, CMG uses Chrome `9222`, Edge `9224`, or Firefox `9223`.

For structured test runs, use `cmg run --browser-port <port>` to target a browser launched on a non-default port.

## Command Groups

- [`browser`](commands/browser/index.md): Browser lifecycle and capture commands.
- [`run`](commands/run.md): Run CMG DSL tests with optional GIFs, reports, traces, and JSON config projects.
- [`api`](commands/api/index.md): HTTP API utility commands.
- [`files`](commands/files/index.md): Local file utility commands.

## Documentation Layout

Command documentation is split by command group:

- Group index files document group purpose, syntax, and subcommands.
- Leaf command files document arguments, options, output, exit codes, and examples.

Scripting documentation lives under [`scripting/`](scripting/index.md).
