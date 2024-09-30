using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using BarFoo.Presentation.ViewModels;
namespace BarFoo.Presentation.Views;

public partial class ProblemBarView : UserControl
{
    public required ProblemBarViewModel ProblemBarVM { get; set; }

    public ProblemBarView()
    {
        InitializeComponent();
        DataContextChanged += ProblemBarView_DataContextChanged;
    }

    private void ProblemBarView_DataContextChanged(object? sender, EventArgs e)
    {
        if (ProblemBarVM != null)
        {
            ProblemBarVM.PropertyChanged -= ViewModel_PropertyChanged;
        }

        ProblemBarVM = DataContext as ProblemBarViewModel;
        if (ProblemBarVM != null)
        {
            ProblemBarVM.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProblemBarViewModel.StatusType))
        {
            UpdateStatusClass();
        }
    }

    private void UpdateStatusClass()
    {
        var statusBorder = this.FindControl<Border>("ProblemBorder");
        if (statusBorder != null && ProblemBarVM != null)
        {
            statusBorder.Classes.Clear();
            switch (ProblemBarVM.StatusType)
            {
                case NotificationType.Error:
                    statusBorder.Classes.Add("Error");
                    break;
                case NotificationType.Warning:
                default:
                    statusBorder.Classes.Add("Warning");
                    break;
            }
        }
    }
}