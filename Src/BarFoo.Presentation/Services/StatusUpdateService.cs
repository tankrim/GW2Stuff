using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;

using Serilog.Core;

namespace BarFoo.Presentation.Services;

public class StatusUpdateService : IStatusUpdateService
{
    private readonly INotificationService _notificationService;

    public StatusUpdateService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void ShowUpdatingNotification()
    {
        _notificationService.UpdateStatus("Updating...", NotificationType.Information);
    }    
}
