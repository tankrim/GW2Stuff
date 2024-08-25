namespace BarFoo.Domain.Entities
{
    public class ApiKeyObjective
    {
        public string ApiKeyName { get; set; }
        public ApiKey ApiKey { get; set; }

        public int ObjectiveId { get; set; }
        public Objective Objective { get; set; }

        public ApiKeyObjective() { }
    }
}