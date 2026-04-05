using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodexThreadViewer.Models;

namespace CodexThreadViewer.Services;

public sealed class CodexThreadStore
{
    private static readonly Regex HistoryPattern = new(
        "^\\{\"session_id\":\"(?<session_id>[^\"]+)\",\"ts\":(?<ts>\\d+),\"text\":\"(?<text>.*)\"\\}$",
        RegexOptions.Compiled);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public async Task<CodexThreadSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var codexHome = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".codex");

        var sessionIndexPath = Path.Combine(codexHome, "session_index.jsonl");
        var historyPath = Path.Combine(codexHome, "history.jsonl");

        if (!Directory.Exists(codexHome))
        {
            throw new DirectoryNotFoundException($".codex folder was not found: {codexHome}");
        }

        if (!File.Exists(sessionIndexPath))
        {
            throw new FileNotFoundException("session_index.jsonl was not found.", sessionIndexPath);
        }

        if (!File.Exists(historyPath))
        {
            throw new FileNotFoundException("history.jsonl was not found.", historyPath);
        }

        var indexedThreads = await ReadSessionIndexAsync(sessionIndexPath, cancellationToken);
        var historyMessages = await ReadHistoryAsync(historyPath, cancellationToken);

        var orderedSessionIds = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var indexedThread in indexedThreads
                     .OrderByDescending(item => item.UpdatedAt ?? DateTimeOffset.MinValue))
        {
            if (seen.Add(indexedThread.Id))
            {
                orderedSessionIds.Add(indexedThread.Id);
            }
        }

        foreach (var historySessionId in historyMessages.Keys
                     .OrderByDescending(sessionId => historyMessages[sessionId].LastOrDefault()?.UtcTimestamp ?? DateTimeOffset.MinValue))
        {
            if (seen.Add(historySessionId))
            {
                orderedSessionIds.Add(historySessionId);
            }
        }

        var indexedById = indexedThreads.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var threads = new List<CodexThreadItem>(orderedSessionIds.Count);

        foreach (var sessionId in orderedSessionIds)
        {
            indexedById.TryGetValue(sessionId, out var indexedThread);
            historyMessages.TryGetValue(sessionId, out var threadMessages);

            threadMessages ??= [];
            var orderedMessages = threadMessages
                .OrderBy(message => message.UtcTimestamp)
                .ToList();

            var threadName = string.IsNullOrWhiteSpace(indexedThread?.ThreadName)
                ? $"thread-{sessionId}"
                : indexedThread.ThreadName;

            threads.Add(new CodexThreadItem(
                sessionId,
                threadName,
                indexedThread?.UpdatedAt,
                orderedMessages.FirstOrDefault()?.UtcTimestamp,
                orderedMessages.LastOrDefault()?.UtcTimestamp,
                orderedMessages));
        }

        return new CodexThreadSnapshot(
            codexHome,
            sessionIndexPath,
            historyPath,
            DateTimeOffset.UtcNow,
            indexedThreads.Count,
            historyMessages.Sum(entry => entry.Value.Count),
            threads);
    }

    public async Task<CodexExportResult> ExportSnapshotAsync(
        CodexThreadSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var outputRoot = Path.Combine(
            desktopPath,
            $"codex-thread-export-{DateTime.Now:yyyyMMdd-HHmmss}");

        Directory.CreateDirectory(outputRoot);
        var threadsDirectory = Directory.CreateDirectory(Path.Combine(outputRoot, "threads"));
        var rawDirectory = Directory.CreateDirectory(Path.Combine(outputRoot, "raw"));

        for (var index = 0; index < snapshot.Threads.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var thread = snapshot.Threads[index];
            var threadDirectory = Directory.CreateDirectory(Path.Combine(
                threadsDirectory.FullName,
                $"{index + 1:D3}_{SanitizeFileName(thread.ThreadName)}"));

            var threadMetadata = new
            {
                session_id = thread.SessionId,
                thread_name = thread.ThreadName,
                updated_at = thread.IndexedUpdatedAt?.ToString("O"),
                first_message_at = thread.FirstMessageAt?.ToString("O"),
                last_message_at = thread.LastMessageAt?.ToString("O"),
                message_count = thread.MessageCount,
                exported_at = DateTimeOffset.UtcNow.ToString("O")
            };

            await File.WriteAllTextAsync(
                Path.Combine(threadDirectory.FullName, "thread.json"),
                JsonSerializer.Serialize(threadMetadata, JsonOptions),
                Encoding.UTF8,
                cancellationToken);

            var messages = thread.Messages.Select(message => new
            {
                session_id = thread.SessionId,
                ts = message.UnixTimestamp,
                utc = message.UtcTimestamp.ToString("O"),
                text = message.Text
            }).ToList();

            await File.WriteAllTextAsync(
                Path.Combine(threadDirectory.FullName, "messages.json"),
                JsonSerializer.Serialize(messages, JsonOptions),
                Encoding.UTF8,
                cancellationToken);

            var messageLines = messages.Select(message => JsonSerializer.Serialize(message));
            await File.WriteAllTextAsync(
                Path.Combine(threadDirectory.FullName, "messages.jsonl"),
                string.Join(Environment.NewLine, messageLines),
                Encoding.UTF8,
                cancellationToken);

            var markdown = new StringBuilder()
                .AppendLine($"# {thread.ThreadName}")
                .AppendLine()
                .AppendLine($"- session_id: `{thread.SessionId}`")
                .AppendLine($"- updated_at: {thread.IndexedUpdatedAt?.ToString("u") ?? "n/a"}")
                .AppendLine($"- message_count: {thread.MessageCount}")
                .AppendLine();

            if (thread.MessageCount == 0)
            {
                markdown.AppendLine("No prompt-history entries were found for this thread.");
            }
            else
            {
                foreach (var message in thread.Messages)
                {
                    markdown.AppendLine($"## {message.UtcTimestamp:u}")
                        .AppendLine()
                        .AppendLine(message.Text)
                        .AppendLine();
                }
            }

            await File.WriteAllTextAsync(
                Path.Combine(threadDirectory.FullName, "messages.md"),
                markdown.ToString(),
                Encoding.UTF8,
                cancellationToken);
        }

        CopyRawFile(snapshot.SessionIndexPath, rawDirectory.FullName);
        CopyRawFile(snapshot.HistoryPath, rawDirectory.FullName);
        CopyPattern(snapshot.CodexHome, "logs_*.sqlite*", rawDirectory.FullName);
        CopyPattern(snapshot.CodexHome, "state_*.sqlite*", rawDirectory.FullName);

        var manifest = new
        {
            exported_at = DateTimeOffset.UtcNow.ToString("O"),
            codex_home = snapshot.CodexHome,
            output_root = outputRoot,
            thread_count = snapshot.Threads.Count,
            parsed_history_entry_count = snapshot.ParsedHistoryEntryCount,
            threads = snapshot.Threads.Select(thread => new
            {
                session_id = thread.SessionId,
                thread_name = thread.ThreadName,
                message_count = thread.MessageCount
            })
        };

        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, "manifest.json"),
            JsonSerializer.Serialize(manifest, JsonOptions),
            Encoding.UTF8,
            cancellationToken);

        return new CodexExportResult(outputRoot, snapshot.Threads.Count);
    }

    private static async Task<List<SessionIndexRow>> ReadSessionIndexAsync(string sessionIndexPath, CancellationToken cancellationToken)
    {
        var rows = new List<SessionIndexRow>();

        await foreach (var line in File.ReadLinesAsync(sessionIndexPath, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var row = JsonSerializer.Deserialize<SessionIndexLine>(line, JsonOptions);
                if (row is null || string.IsNullOrWhiteSpace(row.Id))
                {
                    continue;
                }

                rows.Add(new SessionIndexRow(
                    row.Id,
                    row.ThreadName ?? string.Empty,
                    ParseDateTimeOffset(row.UpdatedAt)));
            }
            catch (JsonException)
            {
                continue;
            }
        }

        return rows;
    }

    private static async Task<Dictionary<string, List<CodexThreadMessage>>> ReadHistoryAsync(
        string historyPath,
        CancellationToken cancellationToken)
    {
        var messagesBySession = new Dictionary<string, List<CodexThreadMessage>>(StringComparer.Ordinal);

        await foreach (var line in File.ReadLinesAsync(historyPath, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var match = HistoryPattern.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var sessionId = match.Groups["session_id"].Value;
            if (!long.TryParse(match.Groups["ts"].Value, CultureInfo.InvariantCulture, out var timestamp))
            {
                continue;
            }

            var message = new CodexThreadMessage(
                timestamp,
                DateTimeOffset.FromUnixTimeSeconds(timestamp),
                NormalizeText(match.Groups["text"].Value));

            if (!messagesBySession.TryGetValue(sessionId, out var bucket))
            {
                bucket = [];
                messagesBySession[sessionId] = bucket;
            }

            bucket.Add(message);
        }

        return messagesBySession;
    }

    private static string NormalizeText(string text)
    {
        return text
            .Replace("\\r\\n", Environment.NewLine)
            .Replace("\\n", Environment.NewLine)
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Trim();
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : null;
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            if (invalid.Contains(character) || char.IsControl(character))
            {
                builder.Append('_');
                continue;
            }

            builder.Append(character);
        }

        var sanitized = builder.ToString().Trim();
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "unnamed-thread";
        }

        return sanitized.Length > 80
            ? sanitized[..80].Trim()
            : sanitized;
    }

    private static void CopyRawFile(string sourcePath, string destinationDirectory)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        File.Copy(
            sourcePath,
            Path.Combine(destinationDirectory, Path.GetFileName(sourcePath)),
            overwrite: true);
    }

    private static void CopyPattern(string directory, string pattern, string destinationDirectory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly))
        {
            File.Copy(
                file,
                Path.Combine(destinationDirectory, Path.GetFileName(file)),
                overwrite: true);
        }
    }

    private sealed record SessionIndexLine(string Id, string? ThreadName, string? UpdatedAt);

    private sealed record SessionIndexRow(string Id, string ThreadName, DateTimeOffset? UpdatedAt);
}
