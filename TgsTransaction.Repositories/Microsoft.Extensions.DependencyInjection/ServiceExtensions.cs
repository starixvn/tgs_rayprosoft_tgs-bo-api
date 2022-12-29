using TGS.Services.TgsTransaction.Core;
using Transaction.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddTgsTransactionRepositoriesServices(this IServiceCollection services)
    {
        _ = services.AddTransient<ITgsTransactionRepository, TransactionRepositoryRepository>();

        return services;
    }
}
