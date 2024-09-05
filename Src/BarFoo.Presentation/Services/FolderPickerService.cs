using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace BarFoo.Presentation.Services;

public class FolderPickerService : IFolderPickerService
{
    public async Task<IStorageFolder?> PickFolderAsync(string title)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Download Directory",
            AllowMultiple = false
        });

        return folders?.Count >= 1 ? folders[0] : null;
    }
}
