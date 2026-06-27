using System.CommandLine;

namespace CMG.Commands;

public sealed record BrowserSelectionOptions(
    Option<bool> Chrome,
    Option<bool> Edge,
    Option<bool> Firefox,
    Option<int?>? Port = null);
