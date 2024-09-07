using Avalonia.Controls.Notifications;

using BarFoo.Core.Interfaces;
using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.Services;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class PactSupplyNetworkAgentViewModel : ViewModelBase
{
    private readonly IPactSupplyNetworkAgentService _pactSupplyNetworkAgentService;
    private readonly IClipboardService _clipboardService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PactSupplyNetworkAgentViewModel> _logger;

    private string _psnaLinks = string.Empty;

    public PactSupplyNetworkAgentViewModel(
        IPactSupplyNetworkAgentService pactSupplyNetworkAgentService,
        IClipboardService clipboardService,
        ILogger<PactSupplyNetworkAgentViewModel> logger,
        INotificationService notificationService)
    {
        _pactSupplyNetworkAgentService = pactSupplyNetworkAgentService ?? throw new ArgumentNullException(nameof(pactSupplyNetworkAgentService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    [RelayCommand]
    public async Task CopyPSNALinksToClipboard()
    {
        try
        {
            _psnaLinks = await _pactSupplyNetworkAgentService.GetPSNA();
            _logger.LogInformation("Retrieved PSNA information.");

            await _clipboardService.SetTextAsync(_psnaLinks);
            _logger.LogInformation("Put PSNA information on the clipboard.");
            _notificationService.UpdateStatus("PSNA Links Copied to Clipboard", NotificationType.Information);
            _notificationService.ShowToast("foo", NotificationType.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while copying PSNA links to clipboard.");
            throw;
        }
    }
}
