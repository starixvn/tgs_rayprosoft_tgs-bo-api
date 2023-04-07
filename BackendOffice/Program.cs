using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapper;
using FluentdForward.OpenTelemetry.MessagePack;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.PlayerBucket.Clients.DependencyInjection;
using TGS.Apis.BackendOffice.Filters;
using TGS.Apis.BackendOffice.MessageQueue;
using TGS.Apis.BackendOffice.Middlewares;
using TGS.Apis.BackendOffice.Services;
using TGS.Apis.BackendOffice.SignalR;
using TGS.Apis.BackendOffice.Swagger;
using TGS.Apis.BackendOffice.Workers;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models;
using TGS.Payment.ManualDeposit;
using TGS.Services.BackOffice.Domain;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
SqlMapper.AddTypeMap(typeof(DateOnly), DbType.Date);

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
            fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
                "MachineInfo",
                () => new
                {
                    Environment.MachineName
                }));
        });
    });

builder.Services
    .AddDataProtection();
builder.Services
    .AddSingleton<IConfigureOptions<KeyManagementOptions>>(
        sp => new ConfigureNamedOptions<KeyManagementOptions>(
            string.Empty,
            options =>
            {
                options.XmlRepository = new RedisXmlRepository(
                    () => sp.GetRequiredService<IRedisConnectionFactory>().GetConnection().GetDatabase(),
                    "Tgs:Bo-Api:DataProtection");
            }));

builder.Services
    .AddRedis((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.ConnectionString = config.GetConnectionString("RedisConfigurationString");
    })
    .AddSignalR();

builder.Services
    .AddHostedService<BatchAdjustNotifyConsumer>()
    .AddHostedService<OfflinePromotionNotifyConsumer>()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
    .AddBackendOfficeServices(builder.Configuration)
    .AddBackendOfficeRepositories(sp => sp
        .GetRequiredService<IRedisConnectionFactory>()
        .GetConnection()
        .GetDatabase())
    .AddBackOfficeCache()
    .AddSingleton<MessageQueueNotifyEventSource>()
    .AddSingleton<INotifyEventSource>(sp => sp.GetRequiredService<MessageQueueNotifyEventSource>())
    .AddMessageQueue()
    .AddNatsMessageQueue(config => config
        .ConfigQueueOptions((options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            options.Url = configuration.GetValue<string>("NatsSetting:Urls");
            options.MaxReconnect = -1;
        })
        .AddReplyHandler<AutoApproveDepositProcessor>(
            "tgs.payment.approve",
            "tgs.payment.approve")
        .AddReplyHandler<AutoApproveDepositProcessor>(
            "tgs.payment.deposit.approve",
            "tgs.payment.deposit.approve")
        .AddReplyHandler<AutoRejectDepositProcessor>(
            "tgs.payment.reject",
            "tgs.payment.reject")
        .AddReplyHandler<AutoRejectDepositProcessor>(
            "tgs.payment.deposit.reject",
            "tgs.payment.deposit.reject")
        .AddReplyHandler<AutoRejectWithdrawProcessor>(
            "tgs.payment.withdraw.reject",
            "tgs.payment.withdraw.reject")
        .AddReplyHandler<AutoPaidWithdrawProcessor>(
            "tgs.payment.withdraw.paid_ex",
            "tgs.payment.withdraw.paid_ex")
        .AddHandler<BatchAdjustBalanceHandler>(
            "tgs.bo.background.adjust_batch",
            "tgs.bo.background.adjust_batch")
        .AddHandler<ProcessOfflineTempUserBatchHandler>(
            "tgs.bo.background.offline-promotion",
            "tgs.bo.background.offline-promotion")
        .AddHandler<BackgroundNotifyHandler>("tgs.bo.notify")
        .AddHandler<CleanBucketsHandler>("tgs.bucket.clean-form.created", "tgs.bo.clean-form")
        .AddHandler<ClearBrandRelationCacheHandler>("tgs.bo.cache.brand-relation.clear"))
    .AddNatsGlobPatternExchange("*");

builder.Services
    .AddPlayerBucketClient(httpClientBuilder => httpClientBuilder
        .ConfigureHttpClient(httpClient => httpClient.BaseAddress =
            builder.Configuration.GetValue<Uri>("BackOffice:PlayerBucketBaseUrl")))
    .AddPaymentServiceClient(httpClientBuilder => httpClientBuilder
        .ConfigureHttpClient((sp, http) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            http.BaseAddress = config.GetValue<Uri>("BackOffice:PaymentServiceBaseUrl");
        }))
    .AddSingleton<ManualDepositSerializer>()
    .AddBrandServiceClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.BrandCertificateBaseUrl = config.GetValue<Uri>("BrandCertificate:Url");
    })
    .AddVipLevelServiceClient(httpClientBuilder => httpClientBuilder
        .ConfigureHttpClient((sp, http) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            http.BaseAddress = config.GetValue<Uri>("BackOffice:VipLevelServiceBaseUrl");
        })
        .AddLoggingHandler<Program>());

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
    .AddHttpContextAccessor()
    .AddRouting(options => options.LowercaseUrls = true);

builder.Services
    .AddDateOnlyTimeOnlyStringConverters()
    .AddFluentValidationAutoValidation(config => config.DisableDataAnnotationsValidation = true)
    .AddFluentValidationClientsideAdapters()
    .AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.Filters.Add(new AuthorizeFilter(policy));
        _ = options.Filters.Add<AccessLogFilter>();
        _ = options.Filters.Add<ValidationFilter>();
    })
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services
    .AddHealthChecks();

builder.Services
    .AddAntiforgery(options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.HeaderName = Consts.BackendOffice.CsrfTokenName;
    });

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/api/auth");
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.Headers["Location"] = context.RedirectUri;
            context.Response.StatusCode = 401;

            return Task.CompletedTask;
        };
    });

builder.Services.AddFileServiceClient((sp, httpClient) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    httpClient.BaseAddress = new Uri(config.GetValue<string>("File:FileServiceBaseUrl"));
});

builder.Services.AddGameProvidersPluggableServerClient((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    options.BaseUrl = config.GetValue<Uri>("BackOffice:GameProvidersPluggableBaseUrl");
});

builder.Services.AddDomainSettingClient((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    options.DomainSettingBaseAddress = config.GetValue<Uri>("BackOffice:DomainSettingBaseAddress");
});

builder.Services.AddPlayerServiceClient((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    options.BaseUrl = config.GetValue<Uri>("BackOffice:PlayerServiceBaseUrl");
});

builder.Services.AddMarketingReportClient((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    options.BaseUrl = config.GetValue<Uri>("BackOffice:MarketingReportBaseUrl");
});

builder.Services.AddDepositAccumulateRouletteClient((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    options.BaseUrl = config.GetValue<Uri>("BackOffice:DepositAccumulateRouletteServiceBaseUrl");
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "TGS BackOffice API",
            Version = "v1"
        });

    c.CustomSchemaIds(type => type.FullName.Replace('.', '_'));
    c.OperationFilter<SwaggerFileOperationFilter>();

    var xml = "TGS.Apis.BackendOffice.xml";
    var path = Path.Combine(AppContext.BaseDirectory, xml);

    if (File.Exists(path))
        c.IncludeXmlComments(path);

    c.UseDateOnlyTimeOnlyStringConverters();
});

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(_ => ResourceBuilder.CreateDefault()
        .AddEnvironmentVariableDetector())
    .WithTracing(tracerProviderBuilder => tracerProviderBuilder
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        })
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .WithMetrics(meterProviderBuilder => meterProviderBuilder
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreInstrumentationOptions>(options =>
    {
        options.Filter = httpContext =>
        {
            return !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/health")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !new[] { "html", "ico", "png", "jpg" }.Any(
                    ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
        };
    });

builder.Services
    .AddProfileViewer();

builder.Services
    .AddHostedService<StationLetterBackground>()
    .AddSingleton<IStationLetterTaskQueue>(sp =>
    {
        if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
        {
            queueCapacity = 100;
        }

        return ActivatorUtilities.CreateInstance<StationLetterTaskQueue>(sp, queueCapacity);
    });

var app = builder.Build();

app.UseRequestLocalization(options =>
{
    options.SupportedCultures.Clear();
    options.SupportedUICultures.Clear();

    builder.Configuration.GetSection("RequestLocalization").Bind(options);

    _ = options.SetDefaultCulture(builder.Configuration.GetValue<string>("RequestLocalization:DefaultRequestCulture"));
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotifyHub>("api/notify");
app.MapProfileViewer();
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Use(async (httpContext, next) =>
{
    var activity = Activity.Current;

    if (activity is not null && !httpContext.Response.Headers.ContainsKey("X-Correlation-ID"))
        _ = httpContext.Response.Headers.TryAdd("X-Correlation-ID", activity.Id.ToString());

    await next();
});

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TGS BackOffice API"));

app.Run();
