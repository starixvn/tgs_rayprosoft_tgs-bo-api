using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using TGS.Apis.BackendOffice;
using TGS.Apis.BackendOffice.DataService.Repositories;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Middlewares;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Extensions;
using TGS.Core.Infrastructures.ApiClient;
using TGS.Core.Infrastructures.Transactions;
using TGS.Core.Infrastructures.WebApiServices;
using TGS.Services.BackOffice.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendOfficeServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTgsRepository(configuration, "Tgs");

        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<ExceptionMiddleware>();

        services.AddTransient(typeof(ITgsCommands<>), typeof(TgsCommands<>));
        services.AddTransient(typeof(ITgsQueries<>), typeof(TgsQueries<>));
        services.AddTransient(typeof(ILogQueries), typeof(LogQueries));
        services.AddTransient<IResellerService, ResellerService>();
        services.AddTransient<ILicenseeService, LicenseeService>();
        services.AddTransient<IBrandPaymentService, BrandPaymentService>();
        services.AddTransient<IStationLetterService, StationLetterService>();

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddSingleton<IFilterFactory, AutoValidateAntiforgeryTokenAttribute>();

        services
            .AddSystemJsonSerializer()
            .AddSecurity()
            .AddWebApiServices()
            .AddTgsLog()
            .AddApiClient()
            .AddTransactions()
            .AddBackOfficeApplicationServices()
            .AddReportClient((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.BaseUrl = config.GetValue<Uri>("Report:Url");
            })
            .AddPromotionClientServices((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                config.GetSection("Promotion").Bind(options);
            })
            .AddPlayerBucketRepositoriesServices()
            .AddCashServiceClient((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.BaseUrl = config.GetValue<Uri>("BackOffice:CashBaseUrl");
            })
            .AddBrandCertificateClient((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.BrandCertificateBaseUrl = config.GetValue<Uri>("BrandCertificate:Url");
            });

        services.AddWallet()
            .AddTgsAgentWalletRepositories();

        services
            .AddPaymentClient();

        return services;
    }

    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> configure)
    {
        services
            .AddOptions<RedisOptions>()
            .Configure(configure)
            .Services
            .AddRedisServices()
            .RegisterRedisConnectionFactory<RedisConnectionFactory>();

        return services;
    }
}
