using BarFoo.Core.DTOs;
using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;
using BarFoo.Presentation.ViewModels;

namespace BarFoo.Presentation.Tests.ViewModels;

[ConstructorTests(typeof(FilterViewModel))]
public partial class FilterViewModelTests
{
    [Fact]
    public void Initialize_SetsDefaultFilterStateValues()
    {
        // Arrange && Act
        AutoMocker mocker = new();       
        FilterViewModel sut = mocker.CreateInstance<FilterViewModel>();

        // Assert
        Assert.False(sut.FilterState.FilterDaily);
        Assert.False(sut.FilterState.FilterWeekly);
        Assert.False(sut.FilterState.FilterSpecial);
        Assert.False(sut.FilterState.FilterPvE);
        Assert.False(sut.FilterState.FilterPvP);
        Assert.False(sut.FilterState.FilterWvW);
        Assert.False(sut.FilterState.FilterNotCompleted);
        Assert.False(sut.FilterState.FilterCompleted);
    }

    [Fact]
    public void HandleApiKeyAdded_AddsNewFilterAndSendsMessage()
    {
        // Arrange       
        AutoMocker mocker = new();
        mocker.Use(new Mock<IMessagingService>());
        mocker.GetMock<IMessagingService>()
            .Setup(x => x.Send(It.IsAny<FilterChangedMessage>()));

        var message = new ApiKeyMessages.ApiKeyAddedMessage(new ApiKeyDto { Name = "TestKey" });
        FilterViewModel sut = mocker.CreateInstance<FilterViewModel>();

        // Act
        sut.HandleApiKeyAdded(this, message);

        // Assert
        Assert.Contains(sut.FilterState.ApiKeyFilters, m => m.ApiKeyName == "TestKey");
    }

    [Fact]
    public void FilterStatePropertyChanged_TriggersSendFilterChangedCommand()
    {
        // Arrange
        AutoMocker mocker = new();

        mocker.Use(new Mock<IMessagingService>());
        mocker.GetMock<IMessagingService>()
            .Setup(x => x.Send(It.IsAny<FilterChangedMessage>())).Verifiable();       

        FilterViewModel sut = mocker.CreateInstance<FilterViewModel>();

        // Act
        sut.FilterState.FilterDaily = true;

        // Assert
        mocker.GetMock<IMessagingService>().Verify(m => m.Send(It.IsAny<FilterChangedMessage>()), Times.Once);
    }
}