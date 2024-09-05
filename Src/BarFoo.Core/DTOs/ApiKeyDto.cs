using System.ComponentModel;

namespace BarFoo.Core.DTOs;

public class ApiKeyDto : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Key { get; set; }
    public bool HasToken { get; set; }
    public bool HasBeenSyncedOnce { get; set; }
    public DateTime LastSyncTime { get; set; }
    public ICollection<ObjectiveDto> Objectives { get; set; } = new List<ObjectiveDto>();

    public ApiKeyDto() { }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateObjectives(IEnumerable<ObjectiveDto> newObjectives)
    {
        Objectives.Clear();
        foreach (var objective in newObjectives)
        {
            Objectives.Add(objective);
        }
        LastSyncTime = DateTime.UtcNow;
        HasBeenSyncedOnce = true;
    }
}