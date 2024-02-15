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
using TGS.Core.Infrastructures.WebApiServices;
using TGS.Services.BackOffice.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendOfficeServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = services.AddTgsRepository(configuration, "Tgs");

        _ = services.AddScoped<IUploadService, UploadService>();
        _ = services.AddScoped<ExceptionMiddleware>();

        _ = services.AddTransient(typeof(ITgsCommands<>), typeof(TgsCommands<>));
        _ = services.AddTransient(typeof(ITgsQueries<>), typeof(TgsQueries<>));
        _ = services.AddTransient(typeof(ILogQueries), typeof(LogQueries));
        _ = services.AddTransient<IResellerService, ResellerService>();
        _ = services.AddTransient<ILicenseeService, LicenseeService>();
        _ = services.AddTransient<IBrandPaymentService, BrandPaymentService>();
        _ = services.AddTransient<IStationLetterService, StationLetterService>();

        _ = services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        _ = services.AddSingleton<IFilterFactory, AutoValidateAntiforgeryTokenAttribute>();

        _ = services
            .AddSystemJsonSerializer()
            .AddSecurity()
            .AddWebApiServices()
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

        _ = services
            .AddPaymentClient();

        return services;
    }

    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> configure)
    {
        _ = services
            .AddOptions<RedisOptions>()
            .Configure(configure)
            .Services
            .AddRedisServices()
            .RegisterRedisConnectionFactory<RedisConnectionFactory>();

        return services;
    }
}
