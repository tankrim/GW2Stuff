using Avalonia.Controls.Notifications;
namespace BarFoo.Presentation.Interfaces;

public interface INotificationService
{
    void ShowToast(string message, NotificationType type = NotificationType.Information);
    void UpdateStatus(string message, NotificationType type = NotificationType.Information);
}
