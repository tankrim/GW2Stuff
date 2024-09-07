using System.Diagnostics;
using System.Drawing;

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
        DataContextChanged += StatusBarView_DataContextChanged;
    }

    private void StatusBarView_DataContextChanged(object? sender, EventArgs e)
    {
        if (VM != null)
        {
            VM.PropertyChanged -= ViewModel_PropertyChanged;
        }

        VM = DataContext as StatusBarViewModel;
        if (VM != null)
        {
            VM.PropertyChanged += ViewModel_PropertyChanged;
            Debug.WriteLine("DataContext set to StatusBarViewModel");
        }
        else
        {
            Debug.WriteLine("DataContext is not StatusBarViewModel");
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StatusBarViewModel.StatusType) ||
            e.PropertyName == nameof(StatusBarViewModel.IsStatusActive))
        {
            Debug.WriteLine($"Property changed: {e.PropertyName}");
            Debug.WriteLine($"StatusType: {VM?.StatusType}, IsStatusActive: {VM?.IsStatusActive}");
            UpdateStatusClass();
        }
    }

    private void UpdateStatusClass()
    {
        var statusBorder = this.FindControl<Border>("StatusBorder");
        if (statusBorder != null && VM != null)
        {
            statusBorder.Classes.Clear();
            switch (VM.StatusType)
            {
                case NotificationType.Error:
                    statusBorder.Classes.Add("Error");
                    Debug.WriteLine("Added Error class to Border");
                    break;
                case NotificationType.Warning:
                    statusBorder.Classes.Add("Warning");
                    Debug.WriteLine("Added Warning class to Border");
                    break;
                case NotificationType.Success:
                    statusBorder.Classes.Add("Success");
                    Debug.WriteLine("Added Success class to Border");
                    break;
                case NotificationType.Information:
                default:
                    statusBorder.Classes.Add("Information");
                    Debug.WriteLine("Added Information class to Border");
                    break;
            }
        }
        else
        {
            Debug.WriteLine("Status is not active, no class added to Border");
        }
    }
}