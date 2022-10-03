using TGS.Services.WalletService.Configuration;
using TGS.Wallet.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class WalletConfigurationExtensions
{
    public static WalletConfiguration AddTgsAgentWalletRepositories(this WalletConfiguration configuration)
        => configuration.AddAgentWalletRepository<AgentWalletRepository>();
}
