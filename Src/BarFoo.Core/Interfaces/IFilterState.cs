namespace BarFoo.Core.Interfaces;

public interface IFilterState
{
    bool FilterDaily { get; }
    bool FilterWeekly { get; }
    bool FilterSpecial { get; }
    bool FilterNotCompleted { get; }
    bool FilterCompleted { get; }
    bool FilterPvE { get; }
    bool FilterPvP { get; }
    bool FilterWvW { get; }
    IReadOnlyList<string> SelectedApiKeys { get; }
}
