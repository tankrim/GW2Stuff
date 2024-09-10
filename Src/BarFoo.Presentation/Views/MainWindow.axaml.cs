using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using BarFoo.Presentation.Interfaces;
using BarFoo.Presentation.Services;

namespace BarFoo.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly INotificationService _notificationService;

    public MainWindow(INotificationService notificationService)
    {
        InitializeComponent();
        Width = 1600;
        Height = 800;
        SizeToContent = SizeToContent.Manual;

        _notificationService = notificationService;
    }

    public MainWindow()
    {
        
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this)!)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3
        };
        (_notificationService as NotificationService)?.SetNotificationManager(notificationManager);
    }
}