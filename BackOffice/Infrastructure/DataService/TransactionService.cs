using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface ITransactionService
{
    Task<Core.DataServices.Tgs.Models.Transaction> GetTransactionAsync(
        Expression<Func<Core.DataServices.Tgs.Models.Transaction, bool>> predicate,
        Func<IQueryable<Core.DataServices.Tgs.Models.Transaction>,
            IQueryable<Core.DataServices.Tgs.Models.Transaction>> preQuery = null);
}

public class TransactionService : BaseRepositoryOperations<ITgsRepository>, ITransactionService
{
    public TransactionService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    public Task<Core.DataServices.Tgs.Models.Transaction> GetTransactionAsync(
        Expression<Func<Core.DataServices.Tgs.Models.Transaction, bool>> predicate,
        Func<IQueryable<Core.DataServices.Tgs.Models.Transaction>,
            IQueryable<Core.DataServices.Tgs.Models.Transaction>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);
}