namespace BarFoo.Domain.Entities;

public class ApiKey
{
    public string Name { get; private set; }
    public string Key { get; private set; }
    public bool HasBeenSyncedOnce { get; set; }
    public DateTime LastSyncTime { get; set; } = DateTime.UtcNow;
    public ICollection<ApiKeyObjective> ApiKeyObjectives { get; set; } = new List<ApiKeyObjective>();

    private ApiKey(string name, string key)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public ApiKey() { }

    public static ApiKey CreateApiKey(string name, string Key)
    {
        return new ApiKey(name.Trim(), Key);
    }

    public void UpdateObjectives(IEnumerable<Objective> newObjectives)
    {
        ApiKeyObjectives.Clear();
        foreach (var objective in newObjectives)
        {
            ApiKeyObjectives.Add(new ApiKeyObjective { ApiKey = this, Objective = objective });
        }
        LastSyncTime = DateTime.UtcNow;
        HasBeenSyncedOnce = true;
    }
}
