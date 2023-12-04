using System;
using Microsoft.Extensions.DependencyInjection;

namespace TGS.Services.WalletService.Configuration;

public class WalletConfiguration(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    public WalletConfiguration AddAgentWalletRepository<TWalletRepository>()
        where TWalletRepository : class, IAgentWalletRepository
    {
        _ = Services.AddTransient<IAgentWalletRepository, TWalletRepository>();

        return this;
    }
}
