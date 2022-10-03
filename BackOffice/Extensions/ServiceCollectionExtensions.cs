using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TGS.Services.BackOffice.Application.Middlewares;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.BackOffice.Infrastructure.RedisServices;
using TGS.Services.BackOffice.Infrastructure.Services;

namespace TGS.Services.BackOffice.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackOfficeApplicationServices(this IServiceCollection services)
    {
        services.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient);
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineBehavior<,>));
        services.AddTransient<IBetLimitService, BetLimitService>();
        services.AddTransient<IBrandService, BrandService>();
        services.AddTransient<IBrandGameService, BrandGameService>();
        services.AddTransient<IBrandGameProviderService, BrandGameProviderService>();
        services.AddTransient<IBrandGameProviderScheduleService, BrandGameProviderScheduleService>();
        services.AddTransient<IGameProviderService, GameProviderService>();
        services.AddTransient<IGameProviderBetLimitService, GameProviderBetLimitService>();
        services.AddTransient<IGameProviderConfigurationService, GameProviderConfigurationService>();
        services.AddTransient<IUploadService, UploadService>();
        services.AddTransient<ICurrencyService, CurrencyService>();
        services.AddTransient<ILanguageService, LanguageService>();
        services.AddTransient<IVipLevelService, VipLevelService>();
        services.AddTransient<IAgentService, AgentService>();
        services.AddTransient<IAgentBankService, AgentBankService>();
        services.AddTransient<IPlayerService, PlayerService>();
        services.AddTransient<ICredentialGenerator, CredentialGenerator>();
        services.AddTransient<ISettingsService, SettingsService>();
        services.AddTransient<IGameService, GameService>();
        services.AddTransient<IWalletExchangeService, WalletExchangeService>();
        services.AddTransient<IPlatformCreditService, PlatformCreditService>();
        services.AddTransient<ISubGameProviderService, SubGameProviderService>();
        services.AddTransient<IMaintenanceRedisService, MaintenanceRedisService>();
        services.AddTransient<IAgentDividendService, AgentDividendService>();
        services.AddTransient<IPlayerIpService, PlayerIpService>();
        services.AddTransient<IPlayerWalletExchangeService, PlayerWalletExchangeService>();
        services.AddTransient<IBrandAgentDividendConditionService, BrandAgentDividendConditionService>();
        services.AddTransient<IBrandAllAgentService, BrandAllAgentService>();
        services.AddTransient<IBrandAllAgentGameTypeService, BrandAllAgentGameTypeService>();
        services.AddTransient<IBrandAllAgentGameTypeConditionService, BrandAllAgentGameTypeConditionService>();
        services.AddTransient<IBrandCurrencyService, BrandCurrencyService>();
        services.AddTransient<IBrandLanguageService, BrandLanguageService>();
        services.AddTransient<IBrandBetLimitService, BrandBetLimitService>();
        services.AddTransient<IBrandGroupService, BrandGroupService>();
        services.AddTransient<IBrandGameProviderAggregateAuditLogService, BrandGameProviderAggregateAuditLogService>();
        services.AddTransient<IGameProviderAggregateAuditLogService, GameProviderAggregateAuditLogService>();
        services.AddTransient<IAdjustAgentAccountService, AdjustAgentAccountService>();
        services.AddTransient<IAdjustPlayerAccountService, AdjustPlayerAccountService>();
        services.AddTransient<IAdjustTransferService, AdjustTransferService>();
        services.AddTransient<IPlayerBucketService, PlayerBucketService>();
        services.AddTransient<ITransactionService, TransactionService>();
        services.AddTransient<IPlayerAdjustBalanceService, PlayerAdjustBalanceService>();
        services.AddSerialNumberGenerator();
        services.AddTgsTransactionRepositoriesServices();

        services.AddSingleton<ISystemClock, DefaultSystemClock>();

        return services;
    }
}
