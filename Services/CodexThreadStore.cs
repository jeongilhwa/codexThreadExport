using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodexThreadViewer.Models;

namespace CodexThreadViewer.Services;

public sealed class CodexThreadStore
{
    private const int ManifestVersion = 1;

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

        return await LoadSnapshotCoreAsync(
            codexHome,
            Path.Combine(codexHome, "session_index.jsonl"),
            Path.Combine(codexHome, "history.jsonl"),
            Path.Combine(codexHome, "sessions"),
            cancellationToken);
    }

    public async Task<CodexImportedSnapshot> LoadExportedSnapshotAsync(
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            throw new ArgumentException("A manifest.json path is required.", nameof(manifestPath));
        }

        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("manifest.json was not found.", manifestPath);
        }

        var packageRoot = Path.GetDirectoryName(manifestPath);
        if (string.IsNullOrWhiteSpace(packageRoot))
        {
            throw new InvalidOperationException("The selected manifest path does not have a valid package root.");
        }

        var manifest = await ReadExportManifestAsync(manifestPath, cancellationToken);
        var rawDirectory = Path.Combine(packageRoot, "raw");
        var snapshot = await LoadSnapshotCoreAsync(
            rawDirectory,
            Path.Combine(rawDirectory, "session_index.jsonl"),
            Path.Combine(rawDirectory, "history.jsonl"),
            Path.Combine(rawDirectory, "sessions"),
            cancellationToken);

        return new CodexImportedSnapshot(
            snapshot,
            packageRoot,
            manifestPath,
            rawDirectory,
            manifest);
    }

    public async Task<CodexExportResult> ExportSnapshotAsync(
        CodexThreadSnapshot snapshot,
        string? destinationDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var exportParent = string.IsNullOrWhiteSpace(destinationDirectory)
            ? desktopPath
            : destinationDirectory;

        if (!Directory.Exists(exportParent))
        {
            throw new DirectoryNotFoundException($"The export destination folder was not found: {exportParent}");
        }

        var outputRoot = Path.Combine(
            exportParent,
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
        CopyDirectory(Path.Combine(snapshot.CodexHome, "sessions"), Path.Combine(rawDirectory.FullName, "sessions"));

        var manifest = new CodexExportManifest(
            ManifestVersion,
            DateTimeOffset.UtcNow.ToString("O"),
            snapshot.CodexHome,
            File.Exists(snapshot.HistoryPath),
            File.Exists(snapshot.SessionIndexPath),
            Directory.Exists(Path.Combine(snapshot.CodexHome, "sessions")),
            snapshot.Threads.Count,
            snapshot.ParsedHistoryEntryCount,
            snapshot.Threads
                .Select(thread => new CodexExportedThreadSummary(
                    thread.SessionId,
                    thread.ThreadName,
                    thread.MessageCount))
                .ToList());

        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, "manifest.json"),
            JsonSerializer.Serialize(manifest, JsonOptions),
            Encoding.UTF8,
            cancellationToken);

        return new CodexExportResult(outputRoot, snapshot.Threads.Count);
    }

    public Task<CodexRestoreResult> RestoreExportAsync(
        string packageRoot,
        string destinationCodexDirectory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(packageRoot))
        {
            throw new ArgumentException("A package root is required.", nameof(packageRoot));
        }

        if (string.IsNullOrWhiteSpace(destinationCodexDirectory))
        {
            throw new ArgumentException("A destination .codex folder is required.", nameof(destinationCodexDirectory));
        }

        var rawDirectory = Path.Combine(packageRoot, "raw");
        if (!Directory.Exists(rawDirectory))
        {
            throw new DirectoryNotFoundException($"The exported snapshot does not contain a raw folder: {rawDirectory}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var destination = Directory.CreateDirectory(destinationCodexDirectory);
        var copiedFileCount = CopyDirectoryContents(rawDirectory, destination.FullName);

        return Task.FromResult(new CodexRestoreResult(destination.FullName, copiedFileCount));
    }

    private static async Task<CodexExportManifest> ReadExportManifestAsync(string manifestPath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var manifest = await JsonSerializer.DeserializeAsync<CodexExportManifest>(stream, JsonOptions, cancellationToken);
        if (manifest is null)
        {
            throw new InvalidDataException($"The manifest could not be parsed: {manifestPath}");
        }

        return manifest;
    }

    private static async Task<CodexThreadSnapshot> LoadSnapshotCoreAsync(
        string codexHome,
        string sessionIndexPath,
        string historyPath,
        string sessionsPath,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(codexHome))
        {
            throw new DirectoryNotFoundException($"The snapshot folder was not found: {codexHome}");
        }

        if (!File.Exists(historyPath))
        {
            throw new FileNotFoundException("history.jsonl was not found.", historyPath);
        }

        var indexedThreads = new List<SessionIndexRow>();
        Dictionary<string, SessionParseResult> sessionRecords = new(StringComparer.Ordinal);

        if (File.Exists(sessionIndexPath))
        {
            indexedThreads = await ReadSessionIndexAsync(sessionIndexPath, cancellationToken);
        }

        if (Directory.Exists(sessionsPath))
        {
            var sessionParseResults = await ReadSessionsDirectoryAsync(sessionsPath, cancellationToken);
            var indexedIds = indexedThreads.Select(row => row.Id).ToHashSet(StringComparer.Ordinal);

            foreach (var result in sessionParseResults)
            {
                if (indexedIds.Add(result.IndexRow.Id))
                {
                    indexedThreads.Add(result.IndexRow);
                }

                if (!sessionRecords.TryGetValue(result.IndexRow.Id, out var existingRecord) ||
                    result.Messages.Count > existingRecord.Messages.Count)
                {
                    sessionRecords[result.IndexRow.Id] = result;
                }
            }
        }

        if (!File.Exists(sessionIndexPath) && !Directory.Exists(sessionsPath))
        {
            throw new FileNotFoundException(
                "Neither session_index.jsonl nor the sessions folder was found.",
                sessionIndexPath);
        }

        var historyMessages = await ReadHistoryAsync(historyPath, cancellationToken);

        var orderedSessionIds = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var indexedById = new Dictionary<string, SessionIndexRow>(StringComparer.Ordinal);

        foreach (var indexedThread in indexedThreads
                     .OrderByDescending(item => item.UpdatedAt ?? DateTimeOffset.MinValue))
        {
            if (seen.Add(indexedThread.Id))
            {
                orderedSessionIds.Add(indexedThread.Id);
                indexedById.Add(indexedThread.Id, indexedThread);
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

        var threads = new List<CodexThreadItem>(orderedSessionIds.Count);

        foreach (var sessionId in orderedSessionIds)
        {
            indexedById.TryGetValue(sessionId, out var indexedThread);
            sessionRecords.TryGetValue(sessionId, out var sessionRecord);
            var threadMessages = sessionRecord?.Messages;
            if (threadMessages is null || threadMessages.Count == 0)
            {
                historyMessages.TryGetValue(sessionId, out threadMessages);
            }

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
                orderedMessages,
                sessionRecord?.WorkingDirectory,
                sessionRecord?.SessionFilePath,
                sessionRecord?.CreatorAccountId,
                sessionRecord?.CreatorUserId));
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

    private static async Task<List<SessionParseResult>> ReadSessionsDirectoryAsync(
        string sessionsPath,
        CancellationToken cancellationToken)
    {
        var rows = new List<SessionParseResult>();

        foreach (var sessionFile in Directory.EnumerateFiles(sessionsPath, "*.jsonl", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await ReadSessionFileAsync(sessionFile, cancellationToken);
            if (result is not null)
            {
                rows.Add(result);
            }
        }

        return rows;
    }

    private static async Task<SessionParseResult?> ReadSessionFileAsync(string sessionFilePath, CancellationToken cancellationToken)
    {
        string? sessionId = null;
        string? threadName = null;
        string? workingDirectory = null;
        string? creatorAccountId = null;
        string? creatorUserId = null;
        DateTimeOffset? updatedAt = null;
        var messages = new List<CodexThreadMessage>();

        await foreach (var line in ReadLinesWithSharedAccessAsync(sessionFilePath, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;

                if (updatedAt is null &&
                    root.TryGetProperty("timestamp", out var timestampElement) &&
                    timestampElement.ValueKind == JsonValueKind.String)
                {
                    updatedAt = ParseDateTimeOffset(timestampElement.GetString());
                }

                if (!root.TryGetProperty("type", out var typeElement) ||
                    typeElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var recordType = typeElement.GetString();
                if (!root.TryGetProperty("payload", out var payloadElement) ||
                    payloadElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (recordType == "session_meta")
                {
                    if (payloadElement.TryGetProperty("id", out var idElement) &&
                        idElement.ValueKind == JsonValueKind.String)
                    {
                        sessionId ??= idElement.GetString();
                    }

                    if (payloadElement.TryGetProperty("timestamp", out var payloadTimestampElement) &&
                        payloadTimestampElement.ValueKind == JsonValueKind.String)
                    {
                        updatedAt ??= ParseDateTimeOffset(payloadTimestampElement.GetString());
                    }

                    workingDirectory ??= ReadNonEmptyString(payloadElement, "cwd");
                    creatorAccountId ??= ReadNonEmptyString(payloadElement, "creator_account_id");
                    creatorUserId ??= ReadNonEmptyString(payloadElement, "creator_user_id");

                    continue;
                }

                if (threadName is null)
                {
                    threadName = TryGetUserMessagePreview(recordType, payloadElement);
                }

                if (TryCreateThreadMessage(root, recordType, payloadElement, out var message))
                {
                    messages.Add(message);
                }
            }
            catch (JsonException)
            {
                continue;
            }
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        updatedAt ??= File.GetLastWriteTimeUtc(sessionFilePath);
        threadName = string.IsNullOrWhiteSpace(threadName)
            ? $"thread-{sessionId}"
            : threadName;

        return new SessionParseResult(
            new SessionIndexRow(sessionId, threadName, updatedAt),
            messages,
            workingDirectory,
            sessionFilePath,
            creatorAccountId,
            creatorUserId);
    }

    private static string? ReadNonEmptyString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static async IAsyncEnumerable<string> ReadLinesWithSharedAccessAsync(
        string filePath,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            yield return line;
        }
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
                "user",
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

    private static string? TryGetUserMessagePreview(string? recordType, JsonElement payloadElement)
    {
        if (recordType == "event_msg" &&
            payloadElement.TryGetProperty("type", out var eventTypeElement) &&
            eventTypeElement.ValueKind == JsonValueKind.String &&
            eventTypeElement.GetString() == "user_message" &&
            payloadElement.TryGetProperty("message", out var messageElement) &&
            messageElement.ValueKind == JsonValueKind.String)
        {
            return ToThreadName(messageElement.GetString());
        }

        if (recordType != "response_item" ||
            !payloadElement.TryGetProperty("type", out var payloadTypeElement) ||
            payloadTypeElement.ValueKind != JsonValueKind.String ||
            payloadTypeElement.GetString() != "message" ||
            !payloadElement.TryGetProperty("role", out var roleElement) ||
            roleElement.ValueKind != JsonValueKind.String ||
            roleElement.GetString() != "user" ||
            !payloadElement.TryGetProperty("content", out var contentElement) ||
            contentElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in contentElement.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var itemTypeElement) ||
                itemTypeElement.ValueKind != JsonValueKind.String ||
                itemTypeElement.GetString() != "input_text" ||
                !item.TryGetProperty("text", out var textElement) ||
                textElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var candidate = ToThreadName(textElement.GetString());
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool TryCreateThreadMessage(
        JsonElement rootElement,
        string? recordType,
        JsonElement payloadElement,
        out CodexThreadMessage message)
    {
        message = default!;

        if (recordType != "response_item" ||
            !payloadElement.TryGetProperty("type", out var payloadTypeElement) ||
            payloadTypeElement.ValueKind != JsonValueKind.String ||
            payloadTypeElement.GetString() != "message" ||
            !payloadElement.TryGetProperty("role", out var roleElement) ||
            roleElement.ValueKind != JsonValueKind.String ||
            !rootElement.TryGetProperty("timestamp", out var timestampElement) ||
            timestampElement.ValueKind != JsonValueKind.String ||
            !DateTimeOffset.TryParse(
                timestampElement.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var timestamp) ||
            !payloadElement.TryGetProperty("content", out var contentElement) ||
            contentElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var role = roleElement.GetString();
        if (role is not ("user" or "assistant"))
        {
            return false;
        }

        var textParts = new List<string>();

        foreach (var item in contentElement.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var itemTypeElement) ||
                itemTypeElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var itemType = itemTypeElement.GetString();
            string? text = itemType switch
            {
                "input_text" when item.TryGetProperty("text", out var inputTextElement) &&
                                  inputTextElement.ValueKind == JsonValueKind.String
                    => inputTextElement.GetString(),
                "output_text" when item.TryGetProperty("text", out var outputTextElement) &&
                                   outputTextElement.ValueKind == JsonValueKind.String
                    => outputTextElement.GetString(),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var normalized = NormalizeText(text);
            if (role == "user" && normalized.StartsWith("<environment_context>", StringComparison.Ordinal))
            {
                continue;
            }

            textParts.Add(normalized);
        }

        var finalText = string.Join(Environment.NewLine + Environment.NewLine, textParts)
            .Trim();
        if (string.IsNullOrWhiteSpace(finalText))
        {
            return false;
        }

        message = new CodexThreadMessage(
            timestamp.ToUnixTimeSeconds(),
            timestamp,
            role,
            finalText);

        return true;
    }

    private static string? ToThreadName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = NormalizeText(value);
        if (normalized.StartsWith("<environment_context>", StringComparison.Ordinal))
        {
            return null;
        }

        var flattened = normalized.ReplaceLineEndings(" ").Trim();
        if (string.IsNullOrWhiteSpace(flattened))
        {
            return null;
        }

        return flattened.Length <= 80
            ? flattened
            : $"{flattened[..77]}...";
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
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly))
        {
            File.Copy(
                file,
                Path.Combine(destinationDirectory, Path.GetFileName(file)),
                overwrite: true);
        }
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        CopyDirectoryContents(sourceDirectory, destinationDirectory);
    }

    private static int CopyDirectoryContents(string sourceDirectory, string destinationDirectory)
    {
        var fileCount = 0;
        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var targetPath = Path.Combine(destinationDirectory, relativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(file, targetPath, overwrite: true);
            fileCount++;
        }

        return fileCount;
    }

    private sealed record SessionIndexLine(string Id, string? ThreadName, string? UpdatedAt);

    private sealed record SessionIndexRow(string Id, string ThreadName, DateTimeOffset? UpdatedAt);

    private sealed record SessionParseResult(
        SessionIndexRow IndexRow,
        List<CodexThreadMessage> Messages,
        string? WorkingDirectory,
        string SessionFilePath,
        string? CreatorAccountId,
        string? CreatorUserId);
}
