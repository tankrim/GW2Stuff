using Avalonia.Platform.Storage;

namespace BarFoo.Presentation.Services;

public interface IFolderPickerService
{
    Task<IStorageFolder?> PickFolderAsync(string title);
}
