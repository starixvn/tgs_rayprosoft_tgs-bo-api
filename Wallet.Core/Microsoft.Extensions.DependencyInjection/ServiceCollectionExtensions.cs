using TGS.Services.WalletService.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static WalletConfiguration AddWallet(this IServiceCollection services)
        => new WalletConfiguration(services);
}
