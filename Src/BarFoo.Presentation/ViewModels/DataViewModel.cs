using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class DataViewModel : ViewModelBase
{
    private readonly ILogger<DataViewModel> _logger;

    public ObjectivesViewModel ObjectivesVM { get; }
    public FilterViewModel FilterVM { get; }

    public DataViewModel(
        ObjectivesViewModel objectivesVM,
        FilterViewModel filterVM,
        ILogger<DataViewModel> logger)
    {
        ObjectivesVM = objectivesVM;
        FilterVM = filterVM;
        _logger = logger;
    }

    public async Task LoadObjectivesAsync()
    {
       await ObjectivesVM.LoadObjectivesAsync();
    }
}