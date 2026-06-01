using Microsoft.Extensions.DependencyInjection;
using Zigm.Services;
using Zigm.Services.Interfaces;

namespace Zigm.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZigmServices(this IServiceCollection services, string? configPath = null)
    {
        services.AddHttpClient();

        services.AddSingleton<IConfigService>(sp => 
        {
            var configService = new ConfigService(configPath);
            configService.Initialize();
            return configService;
        });

        services.AddSingleton<ILocalStorageService>(sp =>
        {
            var configService = sp.GetRequiredService<IConfigService>();
            var config = configService.GetConfig();
            return new LocalStorageService(config);
        });

        services.AddSingleton<IZigVersionService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new ZigVersionService(httpClientFactory);
        });
        services.AddSingleton<IEnvironmentService>(sp =>
        {
            var localStorageService = sp.GetRequiredService<ILocalStorageService>();
            return new EnvironmentService(localStorageService);
        });
        services.AddSingleton<IZigInstallerService>(sp =>
        {
            var localStorageService = sp.GetRequiredService<ILocalStorageService>();
            var zigVersionService = sp.GetRequiredService<IZigVersionService>();
            var environmentService = sp.GetRequiredService<IEnvironmentService>();
            var config = sp.GetRequiredService<IConfigService>().GetConfig();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new ZigInstallerService(localStorageService, zigVersionService, environmentService, config, httpClientFactory);
        });
        services.AddSingleton<ISystemZigService, SystemZigService>();
        services.AddSingleton<IZigmUpdaterService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var localStorageService = sp.GetRequiredService<ILocalStorageService>();
            return new ZigmUpdaterService(httpClientFactory, localStorageService);
        });

        return services;
    }
}
