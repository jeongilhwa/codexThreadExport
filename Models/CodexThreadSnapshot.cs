namespace CodexThreadViewer.Models;

public sealed record CodexThreadSnapshot(
    string CodexHome,
    string SessionIndexPath,
    string HistoryPath,
    DateTimeOffset LoadedAt,
    int IndexedThreadCount,
    int ParsedHistoryEntryCount,
    IReadOnlyList<CodexThreadItem> Threads);

public sealed record CodexThreadItem(
    string SessionId,
    string ThreadName,
    DateTimeOffset? IndexedUpdatedAt,
    DateTimeOffset? FirstMessageAt,
    DateTimeOffset? LastMessageAt,
    IReadOnlyList<CodexThreadMessage> Messages,
    string? WorkingDirectory,
    string? SessionFilePath,
    string? CreatorAccountId,
    string? CreatorUserId)
{
    public int MessageCount => Messages.Count;

    public DateTimeOffset? LastActivityAt => IndexedUpdatedAt ?? LastMessageAt;

    public string Preview
    {
        get
        {
            var text = Messages.LastOrDefault(m => !string.IsNullOrWhiteSpace(m.Text))?.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return "No prompt-history entries found for this thread.";
            }

            var flattened = text.ReplaceLineEndings(" ").Trim();
            return flattened.Length <= 160
                ? flattened
                : $"{flattened[..157]}...";
        }
    }
}

public sealed record CodexThreadMessage(
    long UnixTimestamp,
    DateTimeOffset UtcTimestamp,
    string Role,
    string Text)
{
    public bool IsAssistant => string.Equals(Role, "assistant", StringComparison.OrdinalIgnoreCase);

    public string DisplayRole => IsAssistant ? "Assistant" : "User";
}

public sealed record CodexExportResult(
    string OutputDirectory,
    int ThreadCount);
