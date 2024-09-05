namespace BarFoo.Presentation.Services;

public interface IFileDownloadService
{
    Task DownloadFileAsync(string url, string filePath);
}
