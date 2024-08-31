using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;

using BarFoo.Presentation.Extensions;

namespace BarFoo.Presentation.Services;

public class ClipboardService : IClipboardService
{
    protected virtual IClipboard? Clipboard => _clipboard ??= Application.Current?.GetTopLevel()?.Clipboard;
    private IClipboard? _clipboard;

    public Task<string?> GetTextAsync() => Clipboard?.GetTextAsync() ?? Task.FromResult<string?>(null);
    public Task SetTextAsync(string? text) => Clipboard?.SetTextAsync(text) ?? Task.CompletedTask;
    public Task ClearAsync() => Clipboard?.ClearAsync() ?? Task.CompletedTask;
    public Task SetDataObjectAsync(IDataObject data) => Clipboard?.SetDataObjectAsync(data) ?? Task.CompletedTask;
    public Task<string[]> GetFormatsAsync() => Clipboard?.GetFormatsAsync() ?? Task.FromResult(Array.Empty<string>());
    public Task<object?> GetDataAsync(string format) => Clipboard?.GetDataAsync(format) ?? Task.FromResult<object?>(null);
}
