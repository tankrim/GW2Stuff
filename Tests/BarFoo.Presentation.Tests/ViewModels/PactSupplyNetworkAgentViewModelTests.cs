using BarFoo.Core.Interfaces;
using BarFoo.Presentation.Services;
using BarFoo.Presentation.ViewModels;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.Tests.ViewModels;

[ConstructorTests(typeof(PactSupplyNetworkAgentViewModel))]
public partial class PactSupplyNetworkAgentViewModelTests
{
    [Fact]
    public void CopyPSNALinksToClipboardCommand_OnExecute_GetsDataAndCopiesToClipboard()
    {
        // Arrange

        string expectedData = "[&BIcHAAA=] : [&BEwDAAA=] : [&BNIEAAA=] : [&BKYBAAA=] : [&BIMCAAA=] : [&BA8CAAA=]";
        
        AutoMocker mocker = new();
       
        mocker.Use(new Mock<IPactSupplyNetworkAgentService>(MockBehavior.Strict));

        mocker.GetMock<IPactSupplyNetworkAgentService>()
            .Setup(x => x.GetPSNA())
            .ReturnsAsync(expectedData);

        mocker.Use(new Mock<IClipboardService>(MockBehavior.Strict));

        mocker.GetMock<IClipboardService>()
            .Setup(x => x.SetTextAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mocker.Use(new Mock<ILogger<PactSupplyNetworkAgentViewModel>>());

        PactSupplyNetworkAgentViewModel sut = mocker.CreateInstance<PactSupplyNetworkAgentViewModel>();

        // Act

        sut.CopyPSNALinksToClipboardCommand.Execute(null);

        // Assert

        mocker.Verify<IPactSupplyNetworkAgentService>(x => x.GetPSNA(), Times.Once);
        mocker.Verify<IClipboardService>(x => x.SetTextAsync(expectedData), Times.Once);
    }

    [Fact]
    public async Task CopyPSNALinksToClipboardCommand_OnError_LogsErrorAndPropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Test exception");

        AutoMocker mocker = new();

        mocker.Use(new Mock<IPactSupplyNetworkAgentService>(MockBehavior.Strict));

        mocker.GetMock<IPactSupplyNetworkAgentService>()
            .Setup(x => x.GetPSNA())
            .ThrowsAsync(expectedException);

        mocker.Use(new Mock<IClipboardService>(MockBehavior.Strict));

        mocker.GetMock<IClipboardService>()
            .Setup(x => x.SetTextAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mocker.Use(new Mock<ILogger<PactSupplyNetworkAgentViewModel>>());

        PactSupplyNetworkAgentViewModel sut = mocker.CreateInstance<PactSupplyNetworkAgentViewModel>();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => sut.CopyPSNALinksToClipboardCommand.ExecuteAsync(null));

        // Verify
        mocker.Verify<IPactSupplyNetworkAgentService>(x => x.GetPSNA(), Times.Once);
        mocker.Verify<IClipboardService>(x => x.SetTextAsync(It.IsAny<string>()), Times.Never);
        mocker.Verify<ILogger<PactSupplyNetworkAgentViewModel>>(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while copying PSNA links to clipboard")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once)
            ;
    }
}
