using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.ViewModels;

namespace BarFoo.Presentation.Services;

public class NotificationService : INotificationService
{
    private WindowNotificationManager? _notificationManager;
    private readonly StatusBarViewModel _statusBarViewModel;

    public NotificationService(StatusBarViewModel statusBarViewModel)
    {
        _statusBarViewModel = statusBarViewModel ?? throw new ArgumentNullException(nameof(statusBarViewModel));
    }

    public void SetNotificationManager(WindowNotificationManager notificationManager)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
    }

    //public void ShowToast(string message, NotificationType type = NotificationType.Information)
    //{
    //    var avaloniaType = MapNotificationType(type);
    //    _notificationManager?.Show(new Notification(string.Empty, message, avaloniaType));
    //}

    public void ShowToast(string message, NotificationType type = NotificationType.Information)
    {
        if (_notificationManager is null)
        {
            throw new InvalidOperationException("NotificationManager has not been set.");
        }

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
