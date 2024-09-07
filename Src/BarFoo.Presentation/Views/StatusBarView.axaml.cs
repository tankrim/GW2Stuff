using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using BarFoo.Presentation.ViewModels;

namespace BarFoo.Presentation.Views;

public partial class StatusBarView : UserControl
{
    public required StatusBarViewModel VM { get; set; }

    public StatusBarView()
    {
        InitializeComponent();
    }

    private void UpdateStatusClass()
    {
        var statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
        if (statusTextBlock != null && VM != null)
        {
            statusTextBlock.Classes.Clear();
            switch (VM.StatusType)
            {
                case NotificationType.Error:
                    statusTextBlock.Classes.Add("Error");
                    break;
                case NotificationType.Warning:
                    statusTextBlock.Classes.Add("Warning");
                    break;
                case NotificationType.Success:
                    statusTextBlock.Classes.Add("Success");
                    break;
                default:
                    break;
            }
        }
        
    }
}