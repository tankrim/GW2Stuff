using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.ViewModels;

namespace BarFoo.Presentation.Services;

public class NotificationService : INotificationService
{
    private readonly WindowNotificationManager _notificationManager;
    private readonly StatusBarViewModel _statusBarViewModel;

    public NotificationService(WindowNotificationManager notificationManager, StatusBarViewModel statusBarViewModel)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _statusBarViewModel = statusBarViewModel ?? throw new ArgumentNullException(nameof(statusBarViewModel));
    }

    public void ShowToast(string message, NotificationType type = NotificationType.Information)
    {
        var avaloniaType = MapNotificationType(type);
        _notificationManager.Show(new Notification(string.Empty, message, avaloniaType));
    }

    public void UpdateStatus(string message, NotificationType type = NotificationType.Information)
    {
        _statusBarViewModel.UpdateStatus(message, type);
    }

    private static NotificationType MapNotificationType(NotificationType type) =>
        type switch
        {
            NotificationType.Success => NotificationType.Success,
            NotificationType.Warning => NotificationType.Warning,
            NotificationType.Error => NotificationType.Error,
            _ => NotificationType.Information
        };
}
