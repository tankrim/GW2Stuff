using System.Collections.ObjectModel;

using BarFoo.Core.Interfaces;
using BarFoo.Presentation.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

namespace BarFoo.Presentation.Filters;

public partial class FilterState : ObservableObject, IFilterState
{
    [ObservableProperty] private bool _filterDaily;
    [ObservableProperty] private bool _filterWeekly;
    [ObservableProperty] private bool _filterSpecial;
    [ObservableProperty] private bool _filterNotCompleted;
    [ObservableProperty] private bool _filterCompleted;
    [ObservableProperty] private bool _filterPvE;
    [ObservableProperty] private bool _filterPvP;
    [ObservableProperty] private bool _filterWvW;

    public ObservableCollection<ApiKeyFilter> ApiKeyFilters { get; } = new();

    public IReadOnlyList<string> SelectedApiKeys =>
        ApiKeyFilters.Where(f => f.IsSelected).Select(f => f.ApiKeyName).ToList();
}