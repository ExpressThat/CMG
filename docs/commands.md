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

## Command Groups

- [`browser`](commands/browser/index.md): Browser lifecycle and capture commands.

## Documentation Layout

Command documentation is split by command group:

- Group index files document group purpose, syntax, and subcommands.
- Leaf command files document arguments, options, output, exit codes, and examples.
