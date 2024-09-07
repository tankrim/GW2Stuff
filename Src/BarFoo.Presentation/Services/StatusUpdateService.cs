using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;

namespace BarFoo.Presentation.Services;

public class StatusUpdateService : IStatusUpdateService
{
    private readonly INotificationService _notificationService;

    public StatusUpdateService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void SetIsUpdatingTemporarily()
    {
        _notificationService.UpdateStatus("Updating...", NotificationType.Information);
    }
}
