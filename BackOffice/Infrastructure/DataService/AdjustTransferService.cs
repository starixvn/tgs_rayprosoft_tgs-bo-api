using System;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAdjustTransferService
{
    Task CreateAsync(Guid transactionFromId, Guid transactionToId, AdjustTransferType transferType,
        string transferFromUsername, string transferToUsername, decimal amount);
}

public class AdjustTransferService : BaseRepositoryOperations<ITgsRepository>, IAdjustTransferService
{
    public AdjustTransferService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    public Task CreateAsync(Guid transactionFromId, Guid transactionToId, AdjustTransferType transferType,
        string transferFromUsername, string transferToUsername, decimal amount)
    {
        var vipLevel = new AdjustTransferRecord
        {
            TransactionFromId = transactionFromId,
            TransactionToId = transactionToId,
            TransferType = transferType,
            TransferFromUsername = transferFromUsername,
            TransferToUsername = transferToUsername,
            Amount = amount,
            CreatedOn = DateTimeOffset.UtcNow
        };

        return CreateEntityAsync(vipLevel);
    }
}
