using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Middlewares;
using TGS.Services.BackOffice.Application.Queries.Game;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.BackOffice.Infrastructure.RedisServices;
using TGS.Services.BackOffice.Infrastructure.Services;

namespace TGS.Services.BackOffice.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackOfficeApplicationServices(this IServiceCollection services)
    {
        _ = services.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient);
        _ = services.AddMediatR(r => r.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        _ = services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        _ = services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineBehavior<,>));
        _ = services.AddTransient<IBetLimitService, BetLimitService>();
        _ = services.AddTransient<IBrandService, BrandService>();
        _ = services.AddTransient<IBrandGameService, BrandGameService>();
        _ = services.AddTransient<IBrandGameProviderService, BrandGameProviderService>();
        _ = services.AddTransient<IBrandGameProviderScheduleService, BrandGameProviderScheduleService>();
        _ = services.AddTransient<IGameProviderService, GameProviderService>();
        _ = services.AddTransient<IGameProviderBetLimitService, GameProviderBetLimitService>();
        _ = services.AddTransient<IGameProviderConfigurationService, GameProviderConfigurationService>();
        _ = services.AddTransient<IUploadService, UploadService>();
        _ = services.AddTransient<ICurrencyService, CurrencyService>();
        _ = services.AddTransient<ILanguageService, LanguageService>();
        _ = services.AddTransient<IAgentService, AgentService>();
        _ = services.AddTransient<IAgentBankService, AgentBankService>();
        _ = services.AddTransient<IPlayerService, PlayerService>();
        _ = services.AddTransient<ICredentialGenerator, CredentialGenerator>();
        _ = services.AddTransient<ISettingsService, SettingsService>();
        _ = services.AddTransient<IGameService, GameService>();
        _ = services.AddTransient<IPlatformCreditService, PlatformCreditService>();
        _ = services.AddTransient<ISubGameProviderService, SubGameProviderService>();
        _ = services.AddTransient<IMaintenanceRedisService, MaintenanceRedisService>();
        _ = services.AddTransient<IAgentDividendService, AgentDividendService>();
        _ = services.AddTransient<IPlayerIpService, PlayerIpService>();
        _ = services.AddTransient<IBrandAgentDividendConditionService, BrandAgentDividendConditionService>();
        _ = services.AddTransient<IBrandAllAgentService, BrandAllAgentService>();
        _ = services.AddTransient<IBrandAllAgentGameTypeService, BrandAllAgentGameTypeService>();
        _ = services.AddTransient<IBrandAllAgentGameTypeConditionService, BrandAllAgentGameTypeConditionService>();
        _ = services.AddTransient<IBrandCurrencyService, BrandCurrencyService>();
        _ = services.AddTransient<IBrandLanguageService, BrandLanguageService>();
        _ = services.AddTransient<IBrandBetLimitService, BrandBetLimitService>();
        _ = services.AddTransient<IBrandGroupService, BrandGroupService>();
        _ = services.AddTransient<IBrandGameProviderAggregateAuditLogService, BrandGameProviderAggregateAuditLogService>();
        _ = services.AddTransient<IGameProviderAggregateAuditLogService, GameProviderAggregateAuditLogService>();
        _ = services.AddTransient<IAdjustAgentAccountService, AdjustAgentAccountService>();
        _ = services.AddTransient<IAdjustTransferService, AdjustTransferService>();
        _ = services.AddTransient<IPlayerAdjustBalanceService, PlayerAdjustBalanceService>();
        _ = services.AddSerialNumberGenerator();

        _ = services.AddSingleton<ISystemClock, DefaultSystemClock>();

        return services;
    }
}
