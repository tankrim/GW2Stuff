using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.ViewModels;

namespace BarFoo.Presentation.Services;

public class NotificationService : INotificationService
{
    private readonly InformationBarViewModel _statusBarViewModel;
    private readonly InformationBarViewModel _warningBarViewModel;

    public NotificationService(InformationBarViewModel statusBarViewModel, InformationBarViewModel warningBarViewModel)
    {
        _statusBarViewModel = statusBarViewModel ?? throw new ArgumentNullException(nameof(statusBarViewModel));
        _warningBarViewModel = warningBarViewModel ?? throw new ArgumentNullException(nameof(warningBarViewModel));
    }

    public void UpdateStatus(string message, NotificationType type = NotificationType.Information)
    {
        _statusBarViewModel.UpdateStatus(message, type);
    }
}
