using Avalonia.Controls;

namespace BarFoo.Presentation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Width = 1600;
        Height = 800;
        SizeToContent = SizeToContent.Manual;
    }
}