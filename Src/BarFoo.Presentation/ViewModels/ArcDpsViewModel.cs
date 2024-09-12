using Avalonia.Controls.Notifications;

using BarFoo.Core.Interfaces;
using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ArcDpsViewModel : ViewModelBase
{
    private readonly ILogger<ArcDpsViewModel> _logger;
    private readonly IConfigurationService _configService;
    private readonly IFolderPickerService _folderPickerService;
    private readonly IFileDownloadService _fileDownloadService;
    private readonly INotificationService _notificationService;

    [ObservableProperty] private string? _selectedDirectoryPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadAndSaveFileCommand))]
    private bool _isDownloadEnabled;

    public ArcDpsViewModel(
        ILogger<ArcDpsViewModel> logger,
        IConfigurationService configService,
        IFolderPickerService folderPickerService,
        IFileDownloadService fileDownloadService,
        INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _folderPickerService = folderPickerService ?? throw new ArgumentNullException(nameof(folderPickerService));
        _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        InitializeSelectedDirectoryPath();
    }

    private void InitializeSelectedDirectoryPath()
    {
        SelectedDirectoryPath = _configService.GetSelectedDirectoryPath();
        UpdateDownloadEnabledState();
    }

    public void UpdateDownloadEnabledState()
    {
        IsDownloadEnabled = !string.IsNullOrWhiteSpace(SelectedDirectoryPath);
    }

    [RelayCommand]
    private async Task SelectDirectory()
    {
        try
        {
            var folder = await _folderPickerService.PickFolderAsync("Select Download Directory");
            if (folder is null) return;

            SelectedDirectoryPath = folder.Path.ToString();
            UpdateDownloadEnabledState();
            _logger.LogInformation("Directory selected: {Directory}", SelectedDirectoryPath);

            await _configService.SaveSelectedDirectoryPath(SelectedDirectoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting directory");
            SelectedDirectoryPath = null;
            UpdateDownloadEnabledState();
        }
    }

    [RelayCommand(CanExecute = nameof(IsDownloadEnabled))]
    public async Task DownloadAndSaveFile()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectoryPath))
        {
            _logger.LogError("Download attempted with no directory selected");
            throw new InvalidOperationException("DownloadAndSaveFile was called with no directory selected.");
        }

        try
        {
            var downloadUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll";
            var fileName = "d3d11.dll";

            // Ensure SelectedDirectoryPath is a valid file system path
            var directoryPath = new Uri(SelectedDirectoryPath).LocalPath;
            var filePath = Path.Combine(directoryPath, fileName);

            _notificationService.UpdateStatus("Downloading ArcDPS...", NotificationType.Information);
            await _fileDownloadService.DownloadFileAsync(downloadUrl, directoryPath);
            _logger.LogInformation("File downloaded and saved: {FilePath}", filePath);
            _notificationService.UpdateStatus("ArcDPS download complete.", NotificationType.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DownloadAndSaveFile");
            _notificationService.UpdateStatus("Something went wrong while downloading and saving ArcDPS", NotificationType.Warning);
        }
    }
}