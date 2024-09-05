using System.Reflection;
using System.Text.Json;

using BarFoo.Core.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BarFoo.Core.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configFilePath = Path.Combine(GetBasePath(), "appsettings.json");
    }

    public T? GetSection<T>(string sectionName) where T : class, new()
    {
        var section = _configuration.GetSection(sectionName);
        var result = section.Get<T>();
        _logger.LogInformation("Retrieved section {SectionName}: {@SectionData}", sectionName, result);
        return result;
    }

    public async Task SaveSection<T>(string sectionName, T sectionData) where T : class
    {
        try
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            var jsonDocument = JsonDocument.Parse(json);
            var mutableConfig = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            mutableConfig[sectionName] = JsonSerializer.SerializeToElement(sectionData);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string updatedJson = JsonSerializer.Serialize(mutableConfig, options);

            await File.WriteAllTextAsync(_configFilePath, updatedJson);
            _logger.LogInformation("Configuration section {SectionName} saved successfully", sectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration section {SectionName}", sectionName);
            throw;
        }
    }

    public string? GetSelectedDirectoryPath() => GetSection<AppSettings>("AppSettings")?.SelectedDirectoryPath;

    public Task SaveSelectedDirectoryPath(string path) =>
        SaveSection("AppSettings", new AppSettings { SelectedDirectoryPath = path });

    private static string GetBasePath()
    {
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string basePath = Path.GetDirectoryName(assemblyLocation) ?? AppDomain.CurrentDomain.BaseDirectory;
        return basePath;
    }
}

public class AppSettings
{
    public string? SelectedDirectoryPath { get; set; }
}