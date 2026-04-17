#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;
#endif

namespace CodexThreadViewer.Services;

public interface ICodexTransferPicker
{
    Task<string?> PickExportDestinationDirectoryAsync(CancellationToken cancellationToken = default);

    Task<string?> PickImportManifestFileAsync(CancellationToken cancellationToken = default);

    Task<string?> PickRestoreDestinationDirectoryAsync(CancellationToken cancellationToken = default);
}

public sealed class CodexTransferPicker : ICodexTransferPicker
{
    public Task<string?> PickExportDestinationDirectoryAsync(CancellationToken cancellationToken = default)
    {
        return PickFolderAsync("Select export destination", cancellationToken);
    }

    public Task<string?> PickImportManifestFileAsync(CancellationToken cancellationToken = default)
    {
#if WINDOWS
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List,
            CommitButtonText = "Import snapshot"
        };

        picker.FileTypeFilter.Add(".json");

        InitializeWithWindow.Initialize(picker, GetWindowHandle());
        return PickFileAsync(picker, cancellationToken);
#else
        throw new PlatformNotSupportedException("Snapshot import is currently available on Windows desktop only.");
#endif
    }

    public Task<string?> PickRestoreDestinationDirectoryAsync(CancellationToken cancellationToken = default)
    {
        return PickFolderAsync("Select target .codex folder", cancellationToken);
    }

    private static Task<string?> PickFolderAsync(string commitButtonText, CancellationToken cancellationToken)
    {
#if WINDOWS
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            CommitButtonText = commitButtonText
        };

        picker.FileTypeFilter.Add("*");

        InitializeWithWindow.Initialize(picker, GetWindowHandle());
        return PickFolderCoreAsync(picker, cancellationToken);
#else
        throw new PlatformNotSupportedException("Folder selection is currently available on Windows desktop only.");
#endif
    }

#if WINDOWS
    private static async Task<string?> PickFileAsync(FileOpenPicker picker, CancellationToken cancellationToken)
    {
        var file = await picker.PickSingleFileAsync();
        cancellationToken.ThrowIfCancellationRequested();
        return file?.Path;
    }

    private static async Task<string?> PickFolderCoreAsync(FolderPicker picker, CancellationToken cancellationToken)
    {
        var folder = await picker.PickSingleFolderAsync();
        cancellationToken.ThrowIfCancellationRequested();
        return folder?.Path;
    }

    private static IntPtr GetWindowHandle()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (window is null)
        {
            throw new InvalidOperationException("An active desktop window is required to open the picker.");
        }

        return WindowNative.GetWindowHandle(window);
    }
#endif
}
