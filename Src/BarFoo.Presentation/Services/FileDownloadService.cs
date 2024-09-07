using BarFoo.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.Services;

public class FileDownloadService : IFileDownloadService
{
    private readonly ILogger<IFileDownloadService> _logger;
    private readonly IHttpClientWrapper _httpClient;

    public FileDownloadService(IHttpClientWrapper httpClient, ILogger<IFileDownloadService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DownloadFileAsync(string url, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.LogWarning("Download attempted with no directory selected");
            return;
        }
        try
        {
            _logger.LogInformation("Starting download from {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var fileName = GetFileNameFromResponse(url, response);
            var filePath = Path.Combine(path, fileName);

            // Ensure the directory exists
            _ = Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }
            _logger.LogInformation("File downloaded and saved: {FilePath}", filePath);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error downloading file from {Url}", url);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error saving file to {Directory}", path);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DownloadFileAsync");
            throw;
        }
    }

    private static string GetFileNameFromResponse(string url, HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentDisposition?.FileName is string headerFileName)
        {
            return headerFileName.Trim('"');
        }

        var uri = new Uri(url);
        var fileName = Path.GetFileName(uri.LocalPath);

        return !string.IsNullOrWhiteSpace(fileName) ? fileName : "d3d11.dll";
    }
}