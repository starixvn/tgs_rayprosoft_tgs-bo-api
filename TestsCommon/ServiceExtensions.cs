using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TGS.Core.Infrastructures.Json;

namespace TGS.Tests.TestsCommon;

public static class ServiceExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        _ = services.AddTgsLog();
        _ = services.AddScoped<IJsonSerializationProvider, JsonSerializationProvider>();
        _ = services.AddSingleton<IConfiguration>(configuration);
        return services;
    }
}
