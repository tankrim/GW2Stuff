using System.Reflection;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using BarFoo.Core.Configuration;
using BarFoo.Core.Services;
using BarFoo.Data;
using BarFoo.Data.Contexts;
using BarFoo.Data.Repositories;
using BarFoo.Infrastructure.ApiClients;
using BarFoo.Infrastructure.Mappings;
using BarFoo.Infrastructure.Services;
using BarFoo.Presentation.Services;
using BarFoo.Presentation.ViewModels;
using BarFoo.Presentation.Views;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Serilog;

namespace BarFoo.Presentation;

public partial class App : Application
{

    private const string AppName = DesignTimeDbContextFactory.AppName;
    private const string DbName = DesignTimeDbContextFactory.DbName;

    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _host = CreateHostBuilder().Build();
        ConfigureLogging();
        RegisterUnhandledExceptionHandler();
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await InitializeApplicationAsync(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IHostBuilder CreateHostBuilder(string[] args = null)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(GetBasePath())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<App>();
                }
            })
            .UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
            })
            .ConfigureServices(ConfigureServices);
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        ConfigureDatabase(context, services);
        services.AddAutoMapper(typeof(MappingProfile));
        ConfigureHttpClients(services);
        ConfigureApplicationServices(services);
        ConfigureViewModels(services);
        ConfigureViews(services);
        services.AddSingleton<IAppSettings>(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);
    }

    private async Task InitializeApplicationAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            await _host.StartAsync();
            await EnsureDatabaseCreatedAsync();

            var store = _host.Services.GetRequiredService<IStore>();
            await store.InitializeAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            desktop.MainWindow = mainWindow;

            await mainViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            throw;
        }
    }

    public async Task ShutdownAsync()
    {
        try
        {
            if (_host != null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during application shutdown");
        }
        finally
        {
            Log.Information("Application shutting down");
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(GetBasePath())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("Application logging initialized");
        }
        catch (Exception ex)
        {
            // If we failed to set up logging, use Console as a fallback
            Console.WriteLine($"Failed to initialize logging: {ex}");
            throw;
        }
    }

    private static void RegisterUnhandledExceptionHandler()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled application error");

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Log.Fatal(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }

    private static void ConfigureDatabase(HostBuilderContext context, IServiceCollection services)
    {
        services.AddDbContextFactory<BarFooDbContext>(options =>
        {
            var appDataPath = AppDataPathProvider.GetAppDataPath(AppName);
            var dbPath = Path.Combine(appDataPath, DbName);
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

            if (connectionString != null)
            {
                connectionString = connectionString.Replace("{AppDataPath}", dbPath);
                options.UseSqlite(connectionString);
            }
            else
            {
                throw new InvalidOperationException("Database connection string is missing from configuration.");
            }
        });

        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
    }

    private static void ConfigureHttpClients(IServiceCollection services)
    {
        // Configure and add HTTP clients
        services.AddHttpClient<GuildWars2ApiClient>(client =>
            client.DefaultRequestHeaders.Add("User-Agent", "BarFoo Application"));

        // Register the API client as a scoped service
        services.AddScoped<IGuildWars2ApiClient, GuildWars2ApiClient>();
    }

    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IFetcherService, FetcherService>();
        services.AddSingleton<IStore, Store>();
        services.AddTransient<IClipboardService, ClipboardService>();
        services.AddTransient<IPactSupplyNetworkAgentService, PactSupplyNetworkAgentService>();
        services.AddTransient<IFolderPickerService, FolderPickerService>();
        services.AddHttpClient();
        services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
        services.AddTransient<IFileDownloadService, FileDownloadService>();

        // Register background services
        services.AddHostedService<ApiKeyUpdateService>();

        // Configure logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
    }

    private static void ConfigureViewModels(IServiceCollection services)
    {
        services.AddTransient<StatusBarViewModel>();
        services.AddTransient<ArcDpsViewModel>();
        services.AddTransient<PactSupplyNetworkAgentViewModel>();
        services.AddSingleton<FilterViewModel>();
        services.AddTransient<ObjectivesViewModel>();
        services.AddTransient<ApiKeyViewModel>();
        services.AddTransient<MainViewModel>();
    }

    private static void ConfigureViews(IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
        services.AddTransient<MainView>();
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BarFooDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }

    private static string GetBasePath()
    {
        // Try to get the location of the executing assembly
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string basePath = Path.GetDirectoryName(assemblyLocation)!;

        // If that fails (e.g., in design mode), fall back to the current directory
        if (string.IsNullOrEmpty(basePath))
        {
            basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        return basePath;
    }
}

internal class AppDataPathProvider
{
    public static string GetAppDataPath(string appName)
    {
        ArgumentException.ThrowIfNullOrEmpty(appName, nameof(appName));

        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string fullPath = Path.Combine(appDataPath, appName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }
}

public interface IAppSettings
{
    string? SelectedDirectoryPath { get; set; }
}

internal class AppSettings : IAppSettings
{
    public string? SelectedDirectoryPath { get; set; }
}

internal class ConnectionStrings
{
    public string DefaultConnection { get; set; } = string.Empty;
}