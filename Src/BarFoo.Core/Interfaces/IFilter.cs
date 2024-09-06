namespace BarFoo.Core.Interfaces;

public interface IFilter
{
    bool FilterDaily { get; set; }
    bool FilterWeekly { get; set; }
    bool FilterSpecial { get; set; }
    bool FilterNotCompleted { get; set; }
    bool FilterCompleted { get; set; }
    bool FilterPvE { get; set; }
    bool FilterPvP { get; set; }
    bool FilterWvW { get; set; }
}
