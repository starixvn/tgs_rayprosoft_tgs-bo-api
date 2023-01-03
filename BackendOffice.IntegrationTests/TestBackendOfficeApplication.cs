using System.Runtime.InteropServices;
using HttpOverStream.NamedPipe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry.Exporter;
using TGS.Core.Infrastructures.Redis;

namespace BackendOffice.IntegrationTests;

internal static class TestBackendOfficeApplication
{
    public static IHostBuilder Builder => new FixToolApplication()
        .Builder
        .ConfigureLogging((_, logging) => logging.ClearProviders())
            .ConfigureServices(services => services
                .AddFakeNatsMessageQueue()
                .AddSingleton(Substitute.For<IRedisConnectionFactory>())
                .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
                  sp => new ConfigureNamedOptions<OtlpExporterOptions>(
                      Options.DefaultName,
                      exporterOptions => exporterOptions.TimeoutMilliseconds = 0))
                .AddSingleton<IConfigureOptions<KeyManagementOptions>>(
                    sp => new ConfigureNamedOptions<KeyManagementOptions>(
                        string.Empty,
                        options => options.XmlRepository = null)));

    internal static IHostBuilder UseTestChannelServer(this IHostBuilder hostBuilder, string channelName)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return isWindows
            ? hostBuilder
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .UseHttpOverStreamServer(new NamedPipeListener(channelName)))
            : hostBuilder
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .UseKestrel(options => options.ListenUnixSocket(
                         $"/tmp/{channelName}.sock")));
    }

    private class FixToolApplication : WebApplicationFactory<Program>
    {
        private IHostBuilder? _hostBuilder;

        public IHostBuilder Builder
        {
            get
            {
                _ = Server;
                return _hostBuilder!;
            }
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            _hostBuilder = builder;

            return Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder => webBuilder
                    .UseTestServer())
                .Build();
        }
    }
}
