using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.TgsTransaction.Core;

namespace Transaction.Repositories;

public class TransactionRepositoryRepository : BaseRepositoryOperations<ITgsRepository>, ITgsTransactionRepository
{
    private readonly ITgsRepository _repository;

    public TransactionRepositoryRepository(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
        _repository = tgsRepository;
    }

    Task ITgsTransactionRepository.CreateTransactionPromotionsAsync(TransactionPromotion[] transactionPromotions) =>
        CreateEntitiesAsync(transactionPromotions);
}