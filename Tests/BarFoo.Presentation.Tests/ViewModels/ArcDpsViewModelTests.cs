using System.Linq;

using Avalonia.Platform.Storage;

using BarFoo.Core.Interfaces;
using BarFoo.Presentation.Services;
using BarFoo.Presentation.ViewModels;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.Tests.ViewModels;

[ConstructorTests(typeof(ArcDpsViewModel))]
public partial class ArcDpsViewModelTests
{
    [Fact]
    public void InitializeSelectedDirectoryPath_SetsPathAndUpdatesDownloadState()
    {
        // Arrange
        AutoMocker mocker = new();
        
        const string expectedPath = "C:\\TestPath";      

        mocker.Use( new Mock<IConfigurationService>());
        mocker.GetMock<IConfigurationService>()
            .Setup(x => x.GetSelectedDirectoryPath()).Returns(expectedPath);

        // Act
        ArcDpsViewModel sut = mocker.CreateInstance<ArcDpsViewModel>();

        // Assert
        Assert.Equal(expectedPath, sut.SelectedDirectoryPath);
        Assert.True(sut.IsDownloadEnabled);
    }

    [Fact]
    public async Task SelectDirectory_UpdatesPathAndSavesConfiguration()
    {
        // Arrange
        AutoMocker mocker = new();
        
        Uri uri = new("C:\\NewPath\\Test.test");

        var mockFolder = new Mock<IStorageFolder>();
        mockFolder.Setup(folder => folder.Path).Returns(uri);

        mocker.Use(new Mock<IFolderPickerService>());
        mocker.GetMock<IFolderPickerService>()
            .Setup(x => x.PickFolderAsync(It.IsAny<string>()))
            .ReturnsAsync(mockFolder.Object);

        ArcDpsViewModel sut = mocker.CreateInstance<ArcDpsViewModel>();
        
        // Act
        await sut.SelectDirectoryCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("file:///C:/NewPath/Test.test", sut.SelectedDirectoryPath);
        mocker.GetMock<IConfigurationService>()
            .Verify(x => x.SaveSelectedDirectoryPath("file:///C:/NewPath/Test.test"), Times.Once);
    }

    // TODO: Commented out because I don't want to test logging, and we currently don't do anything besides return in this case
    //[Fact]
    //public async Task DownloadAndSaveFile_NoDirectorySelected_LogsWarning()
    //{
    //    // Arrange
    //    AutoMocker mocker = new();
    //    mocker.Use(new Mock<ILogger<ArcDpsViewModel>>());

    //    ArcDpsViewModel sut = mocker.CreateInstance<ArcDpsViewModel>();
    //    sut.SelectedDirectoryPath = string.Empty;

    //    // Act
    //    await sut.DownloadAndSaveFile();

    //    // Assert
    //    mocker.Verify<ILogger<ArcDpsViewModel>>(
    //        x => x.Log(
    //            LogLevel.Warning,
    //            It.IsAny<EventId>(),
    //            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Download attempted with no directory selected")),
    //            null,
    //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    //        Times.Once)
    //        ;
    //}

    [Fact]
    public async Task DownloadAndSaveFile_ValidDirectory_DownloadsFile()
    {
        // Arrange
        AutoMocker mocker = new();
        var directoryPath = new Uri("C:\\TestDirectory").LocalPath;
        mocker.Use(new Mock<IFileDownloadService>(MockBehavior.Strict));
        mocker.GetMock<IFileDownloadService>()
               .Setup(service => service.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(Task.CompletedTask);
        ArcDpsViewModel sut = mocker.CreateInstance<ArcDpsViewModel>();
        sut.SelectedDirectoryPath = directoryPath;

        // Act
        await sut.DownloadAndSaveFile();

        // Assert
        mocker.GetMock<IFileDownloadService>()
               .Verify(x => x.DownloadFileAsync(
                   "https://www.deltaconnected.com/arcdps/x64/d3d11.dll",
                   directoryPath),
                   Times.Once);
    }

    // TODO: Commented out because I don't want to test logging, and we currently don't do anything besides return in this case
    //[Fact]
    //public async Task DownloadAndSaveFile_ExceptionThrown_LogsError()
    //{
    //    // Arrange
    //    AutoMocker mocker = new();

    //    var directoryPath = "C:\\TestDirectory";
    //    var expectedException = new Exception("Test exception");

    //    mocker.Use(new Mock<IFileDownloadService>(MockBehavior.Strict));
    //    mocker.GetMock<IFileDownloadService>()
    //           .Setup(service => service.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
    //           .ThrowsAsync(expectedException);

    //    mocker.Use(new Mock<ILogger<ArcDpsViewModel>>());

    //    ArcDpsViewModel sut = mocker.CreateInstance<ArcDpsViewModel>();
    //    sut.SelectedDirectoryPath = directoryPath;

    //    // Act
    //    await sut.DownloadAndSaveFile();

    //    // Assert
    //    mocker.Verify<ILogger<ArcDpsViewModel>>(
    //        x => x.Log(
    //            LogLevel.Error,
    //            It.IsAny<EventId>(),
    //            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in DownloadAndSaveFile")),
    //            expectedException,
    //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    //        Times.Once)
    //        ;
    //}
}
