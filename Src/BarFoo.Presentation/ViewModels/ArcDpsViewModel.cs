using BarFoo.Core.Interfaces;
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

    [ObservableProperty] private string? _selectedDirectoryPath;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadAndSaveFileCommand))]
    private bool _isDownloadEnabled;

    public ArcDpsViewModel(
        ILogger<ArcDpsViewModel> logger,
        IConfigurationService configService,
        IFolderPickerService folderPickerService,
        IFileDownloadService fileDownloadService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _folderPickerService = folderPickerService ?? throw new ArgumentNullException(nameof(folderPickerService));
        _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));

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
            _logger.LogWarning("Download attempted with no directory selected");
            return;
        }

        try
        {
            var downloadUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll";
            var fileName = "d3d11.dll";
            var filePath = Path.Combine(SelectedDirectoryPath, fileName);

            await _fileDownloadService.DownloadFileAsync(downloadUrl, filePath);

            _logger.LogInformation("File downloaded and saved: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DownloadAndSaveFile");
        }
    }
}