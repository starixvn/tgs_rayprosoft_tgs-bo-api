using Tgs.BrandCertificate.Clients;
using Tgs.MarketingReport.Client;
using TGS.Services.BackOffice;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Services;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Cache;
using TGS.Services.BackOffice.Domain;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackOfficeCache(this IServiceCollection services)
        => services
            .Decorate<IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics>,
                CachedAgentPlayerDepositQueryService>()
            .Decorate<IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem>,
                CachedPlayerPaymentStatisticsDetailQueryService>()
            .Decorate<IQueryService<AgentMarketingReportQuery, DailyAggregateInfo>,
                CachedAgentMarketingReportQueryService>()
            .Decorate<IQueryService<AgentPlayerGamingDetailQuery, PlayerGamingDetail>,
                CachedAgentPlayerGamingDetailQueryService>()
            .Decorate<IQueryService<AgentPlayerRegisterDetailQuery, PlayerRegisterDetail>,
                CachedAgentPlayerRegisterDetailQueryService>()
            .Decorate<IQueryService<AllAgentsByBrandQuery, AgentInfo>, CachedAllAgentsByBrandQueryService>()
            .Decorate<IQueryService<BrandRelationQuery, BrandRelationInfo>, CachedBrandRelationQueryService>()
            .AddTransient<IClearBrandRelationCacheService, CachedBrandRelationQueryService>();
}
