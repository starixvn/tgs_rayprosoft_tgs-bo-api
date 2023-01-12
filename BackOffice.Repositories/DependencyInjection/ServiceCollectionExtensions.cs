using System;
using StackExchange.Redis;
using TGS.BackOffice.Repositories;
using TGS.Services.BackOffice.Domain;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendOfficeRepositories(this IServiceCollection services,
        Func<IServiceProvider, IDatabase> redisDatabaseResolver)
    {
        _ = services
            .AddTransient<ISettingRepository, SettingRepository>()
            .AddTransient<IBackOfficeRepository, BackOfficeRepository>()
            .AddTransient<IAgentRepository, AgentRepository>()
            .AddTransient<IPaymentReportingRepository, PaymentReportingRepository>()
            .AddTransient<IMarketingReportRepository, MarketingReportRepository>()
            .AddTransient<IGameRepository, GameRepository>();

        _ = services
            .AddTransient<IPlayerOnlineRepository>(
                sp => ActivatorUtilities.CreateInstance<PlayerOnlineRepository>(
                    sp,
                    redisDatabaseResolver(sp)))
            .AddTransient<IBatchTempRepository>(
                sp => ActivatorUtilities.CreateInstance<RedisBatchTempRepository>(
                    sp,
                    redisDatabaseResolver(sp)));

        return services;
    }
}
