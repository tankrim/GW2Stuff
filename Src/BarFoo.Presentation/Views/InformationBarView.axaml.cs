using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using BarFoo.Presentation.ViewModels;
namespace BarFoo.Presentation.Views;

public partial class InformationBarView : UserControl
{
    public required InformationBarViewModel InformationBarVM { get; set; }

    public InformationBarView()
    {
        InitializeComponent();
        DataContextChanged += ProblemBarView_DataContextChanged;
    }

    private void ProblemBarView_DataContextChanged(object? sender, EventArgs e)
    {
        if (InformationBarVM != null)
        {
            InformationBarVM.PropertyChanged -= ViewModel_PropertyChanged;
        }

        InformationBarVM = DataContext as InformationBarViewModel;
        if (InformationBarVM != null)
        {
            InformationBarVM.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InformationBarViewModel.StatusType))
        {
            UpdateStatusClass();
        }
    }

    private void UpdateStatusClass()
    {
        var statusBorder = this.FindControl<Border>("InformationBorder");
        if (statusBorder != null && InformationBarVM != null)
        {
            statusBorder.Classes.Clear();
            switch (InformationBarVM.StatusType)
            {
                case NotificationType.Success:
                    statusBorder.Classes.Add("Success");
                    break;
                case NotificationType.Information:
                default:
                    statusBorder.Classes.Add("Information");
                    break;
            }
        }
    }
}