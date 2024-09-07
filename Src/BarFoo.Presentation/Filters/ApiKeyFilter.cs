using CommunityToolkit.Mvvm.ComponentModel;

namespace BarFoo.Presentation.ViewModels;

public partial class ApiKeyFilter : ObservableObject
{
    public string ApiKeyName { get; }

    [ObservableProperty]
    private bool _isSelected;

    public ApiKeyFilter(string apiKeyName)
    {
        ApiKeyName = apiKeyName;
        IsSelected = true;
    }
}