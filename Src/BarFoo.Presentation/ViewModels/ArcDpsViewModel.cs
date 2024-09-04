using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

using BarFoo.Core.Configuration;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ArcDpsViewModel : ViewModelBase
{
    private readonly ILogger<ArcDpsViewModel> _logger;
    private readonly string _downloadUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll";
    private readonly HttpClient _httpClient = new();
    private readonly IConfigurationService _configService;

    [ObservableProperty] private string? _selectedDirectoryPath;
    [ObservableProperty] private bool _isDownloadEnabled;

    public ArcDpsViewModel(ILogger<ArcDpsViewModel> logger, IConfigurationService configService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        IsDownloadEnabled = !string.IsNullOrWhiteSpace(SelectedDirectoryPath);

        InitializeSelectedDirectoryPath();
    }

    private void InitializeSelectedDirectoryPath()
    {
        SelectedDirectoryPath = _configService.GetSelectedDirectoryPath();
        IsDownloadEnabled = !string.IsNullOrWhiteSpace(SelectedDirectoryPath);
    }

    [RelayCommand]
    private async Task SelectDirectory()
    {
        try
        {
            var folder = await DoOpenFolderPickerAsync();
            if (folder is null) return;

            SelectedDirectoryPath = folder.Path.LocalPath;
            IsDownloadEnabled = true;
            _logger.LogInformation("Directory selected: {Directory}", SelectedDirectoryPath);

            await _configService.SaveSelectedDirectoryPath(SelectedDirectoryPath);
        }
        catch (Exception ex)
        {
            IsDownloadEnabled = false;
            _logger.LogError(ex, "Error selecting directory");
        }
    }

    [RelayCommand]
    private async Task DownloadAndSaveFile()
    {
        if (!IsDownloadEnabled)
        {
            _logger.LogWarning("Download attempted while disabled");
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(SelectedDirectoryPath))
            {
                throw new InvalidOperationException("No directory selected.");
            }

            _logger.LogInformation("Starting download from {Url}", _downloadUrl);
            var response = await _httpClient.GetAsync(_downloadUrl);
            response.EnsureSuccessStatusCode();

            var fileName = GetFileNameFromResponse(response);
            var filePath = Path.Combine(SelectedDirectoryPath, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            _logger.LogInformation("File downloaded and saved: {FilePath}", filePath);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error downloading file from {Url}", _downloadUrl);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error saving file to {Directory}", SelectedDirectoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DownloadAndSaveFile");
        }
    }

    private string GetFileNameFromResponse(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentDisposition?.FileName is string headerFileName)
        {
            return headerFileName.Trim('"');
        }

        var uri = new Uri(_downloadUrl);
        var fileName = Path.GetFileName(uri.LocalPath);

        return !string.IsNullOrWhiteSpace(fileName) ? fileName : "d3d11.dll";
    }

    private static async Task<IStorageFolder?> DoOpenFolderPickerAsync()
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