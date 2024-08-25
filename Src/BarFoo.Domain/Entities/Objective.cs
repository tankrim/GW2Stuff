namespace BarFoo.Domain.Entities;

public class Objective
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Track { get; set; }
    public int Acclaim { get; set; }
    public int ProgressCurrent { get; set; }
    public int ProgressComplete { get; set; }
    public bool Claimed { get; set; }
    public string ApiEndpoint { get; set; }

    public ICollection<ApiKeyObjective> ApiKeyObjectives { get; set; } = new List<ApiKeyObjective>();

    public Objective() { }

}
