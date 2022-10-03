using System;
using Microsoft.Extensions.DependencyInjection;

namespace TGS.Services.WalletService.Configuration;

public class WalletConfiguration
{
    public IServiceCollection Services { get; }

    public WalletConfiguration(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public WalletConfiguration AddAgentWalletRepository<TWalletRepository>()
        where TWalletRepository : class, IAgentWalletRepository
    {
        Services.AddTransient<IAgentWalletRepository, TWalletRepository>();

        return this;
    }
}
