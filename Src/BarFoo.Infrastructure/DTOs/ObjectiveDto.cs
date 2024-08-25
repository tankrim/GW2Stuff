namespace BarFoo.Infrastructure.DTOs;

public class ObjectiveDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Track { get; set; }
    public int Acclaim { get; set; }
    public int ProgressCurrent { get; set; }
    public int ProgressComplete { get; set; }
    public bool Claimed { get; set; }
    public string ApiEndpoint { get; set; }
    public string ApiKeyName { get; set; }

    public ObjectiveDto() { }

    public ObjectiveDto(int id, string title, string track, int acclaim, int progressCurrent, int progressComplete, bool claimed, string apiEndpoint, string apiKeyName)
    {
        Id = id;
        Title = title;
        Track = track;
        Acclaim = acclaim;
        ProgressCurrent = progressCurrent;
        ProgressComplete = progressComplete;
        Claimed = claimed;
        ApiEndpoint = apiEndpoint;
        ApiKeyName = apiKeyName;
    }
}
