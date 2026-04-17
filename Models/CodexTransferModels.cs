namespace CodexThreadViewer.Models;

public sealed record CodexImportedSnapshot(
    CodexThreadSnapshot Snapshot,
    string PackageRoot,
    string ManifestPath,
    string RawDirectory,
    CodexExportManifest Manifest)
{
    public bool CanRestoreRawFiles => Directory.Exists(RawDirectory);
}

public sealed record CodexRestoreResult(
    string DestinationDirectory,
    int CopiedFileCount);

public sealed record CodexExportManifest(
    int ManifestVersion,
    string ExportedAt,
    string OriginalCodexHome,
    bool IncludesHistory,
    bool IncludesSessionIndex,
    bool IncludesSessionsDirectory,
    int ThreadCount,
    int ParsedHistoryEntryCount,
    IReadOnlyList<CodexExportedThreadSummary> Threads);

public sealed record CodexExportedThreadSummary(
    string SessionId,
    string ThreadName,
    int MessageCount);
