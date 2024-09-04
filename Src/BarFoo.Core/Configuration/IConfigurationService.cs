namespace BarFoo.Core.Configuration
{
    public interface IConfigurationService
    {
        string? GetSelectedDirectoryPath();
        Task SaveSelectedDirectoryPath(string path);
        T? GetSection<T>(string sectionName) where T : class, new();
        Task SaveSection<T>(string sectionName, T sectionData) where T : class;
    }
}