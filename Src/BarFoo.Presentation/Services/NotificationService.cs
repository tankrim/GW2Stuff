using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.ViewModels;

namespace BarFoo.Presentation.Services;

public class NotificationService : INotificationService
{
    private readonly InformationBarViewModel _informationBarVM;
    private readonly ProblemBarViewModel _problemBarVM;

    public NotificationService(InformationBarViewModel informationBarViewModel, ProblemBarViewModel problemBarViewModel)
    {
        _informationBarVM = informationBarViewModel ?? throw new ArgumentNullException(nameof(informationBarViewModel));
        _problemBarVM = problemBarViewModel ?? throw new ArgumentNullException(nameof(problemBarViewModel));
    }

    public void UpdateStatus(string message, NotificationType type = NotificationType.Information)
    {
        switch (type)
        {
            case NotificationType.Information:
            case NotificationType.Success:
                _informationBarVM.UpdateStatus(message, type);
                break;
            case NotificationType.Warning:
            case NotificationType.Error:
                _problemBarVM.UpdateStatus(message, type);
                break;
            default:
                break;
        }
    }
}