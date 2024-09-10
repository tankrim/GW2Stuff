using Avalonia.Controls.Notifications;
namespace BarFoo.Presentation.Interfaces;

public interface INotificationService
{
    void UpdateStatus(string message, NotificationType type = NotificationType.Information);
}