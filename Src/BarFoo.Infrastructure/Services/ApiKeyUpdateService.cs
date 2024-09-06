using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BarFoo.Infrastructure.Services;

public class ApiKeyUpdateService : BackgroundService
{
    private readonly IStore _store;
    private readonly ILogger<ApiKeyUpdateService> _logger;
    private readonly IMessagingService _messagingService;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(15);

    public ApiKeyUpdateService(IStore store, ILogger<ApiKeyUpdateService> logger, IMessagingService messagingService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ApiKey Update Service is starting.");

        await WaitForStoreInitializationAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            _messagingService.Send(new IsUpdatingMessage(true));

            _logger.LogInformation("ApiKey Update Service is running update operation.");

            await UpdateAllApiKeysAsync(cancellationToken);

            _messagingService.Send(new IsUpdatingMessage(false));

            _logger.LogInformation("ApiKey Update Service is waiting for next interval.");
            await Task.Delay(_updateInterval, cancellationToken);
        }

        _logger.LogInformation("ApiKey Update Service is stopping.");
    }

    private async Task WaitForStoreInitializationAsync(CancellationToken cancellationToken)
    {

        while (!_store.IsInitialized && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Waiting for Store to be initialized...");
            await Task.Delay(1000, cancellationToken); // Wait for 1 second before checking again
        }

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Cancellation requested while waiting for Store initialization");
            return;
        }

        _logger.LogInformation("Store is initialized. ApiKey Update Service can proceed.");
    }

    private async Task UpdateAllApiKeysAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _store.SyncObjectivesForAllApiKeysAsync();
            _logger.LogInformation("ApiKey Update Service completed scheduled update of all apikeys");
            _messagingService.Send(new ApiKeyMessages.ApiKeysUpdatedMessage());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in ApiKey Update Service");
        }
    }
}