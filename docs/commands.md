# CMG Command Reference

CMG is a CLI for controlling a browser and capturing browser content. It is intended to be called by AI agents, so command output should stay predictable and parseable.

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

## Command Groups

- [`browser`](commands/browser/index.md): Browser lifecycle and capture commands.
- [`run`](commands/run.md): Run CMG DSL tests with optional GIF recordings.

## Documentation Layout

Command documentation is split by command group:

- Group index files document group purpose, syntax, and subcommands.
- Leaf command files document arguments, options, output, exit codes, and examples.

Scripting documentation lives under [`scripting/`](scripting/index.md).
