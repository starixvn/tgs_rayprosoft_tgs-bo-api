using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Infrastructures.Log;

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

        services.AddTgsLog();
        services.AddScoped<IJsonSerializationProvider, JsonSerializationProvider>();
        services.AddSingleton<IConfiguration>(configuration);
        return services;
    }
}