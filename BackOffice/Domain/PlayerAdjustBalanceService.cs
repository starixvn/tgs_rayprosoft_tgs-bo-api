using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.SerialNumberGenerator;
using Tgs.Service.PlayerBuckets;
using TGS.Core.Models.Enums;
using TGS.Services.WalletService;
using BucketType = Tgs.Service.PlayerBuckets.BucketType;

namespace TGS.Services.BackOffice.Domain;

internal class PlayerAdjustBalanceService : IPlayerAdjustBalanceService
{
    private readonly IAgentWalletRepository _agentWalletRepository;
    private readonly IBackOfficeRepository _backOfficeRepository;
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly ISerialNumberGenerator _serialNumberGenerator;

    public PlayerAdjustBalanceService(
        IBackOfficeRepository backOfficeRepository,
        ICashServiceClient cashServiceClient,
        IPlayerBucketClient playerBucketClient,
        ISerialNumberGenerator serialNumberGenerator,
        IAgentWalletRepository agentWalletRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
        _agentWalletRepository = agentWalletRepository ?? throw new ArgumentNullException(nameof(agentWalletRepository));
    }

    public async ValueTask ProcessAdjustApplyAsync(
        PlayerBalanceAdjustmentApplyInfo applyInfo,
        Guid modifyUserId,
        string modifyUser,
        CancellationToken cancellationToken = default)
    {
        if (applyInfo.BucketAmount > 0M)
            _ = await _playerBucketClient.CreateBucketAsync(
                applyInfo.PlayerCode,
                applyInfo.BucketAmount,
                BucketType.Adjustment,
                null,
                cancellationToken).ConfigureAwait(false);

        var transactionId = applyInfo.Amount > 0M
            ? (await AdjustAsync(
                applyInfo.TransactionType,
                applyInfo.WalletId,
                applyInfo.AdjustmentType == AdjustmentType.Subtract
                    ? -applyInfo.Amount
                    : applyInfo.Amount,
                applyInfo.Remark,
                modifyUserId).ConfigureAwait(false)).TransactionId
            : (Guid?)null;

        var recordCreateInfo = new AdjustPlayerRecordCreateInfo(
            applyInfo.BrandId,
            applyInfo.PlayerCode,
            applyInfo.AdjustmentType,
            applyInfo.TransactionType,
            applyInfo.TransferType,
            applyInfo.Amount,
            applyInfo.BucketAmount,
            applyInfo.IncludeInNetProfit,
            applyInfo.Remark,
            transactionId,
            null);

        await _backOfficeRepository.InsertAdjustPlayerRecordAsync(
            recordCreateInfo,
            modifyUser,
            cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask ProcessLockedAmountAdjustApplyAsync(
        Guid walletId,
        decimal lockAmount,
        string remark,
        Guid modifyUserId,
        CancellationToken cancellationToken = default)
    {
        _ = await _cashServiceClient.AdjustSystemV2Async(
            walletId,
            0M,
            lockAmount,
            remark,
            null,
            modifyUserId,
            cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask ProcessTransferApplyAsync(
            PlayerBalanceTransferApplyInfo applyInfo,
        Guid modifyUserId,
        CancellationToken cancellationToken = default)
    {
        if (applyInfo.BucketAmount > 0M)
            _ = await _playerBucketClient.CreateBucketAsync(
                applyInfo.PlayerCode,
                applyInfo.BucketAmount,
                BucketType.Adjustment,
                null,
                cancellationToken).ConfigureAwait(false);

        var createId = await _serialNumberGenerator
            .GenerateAsync(cancellationToken)
            .ConfigureAwait(false);

        var lockResult = await _cashServiceClient.LockTransferAsync(
            applyInfo.WalletId,
            applyInfo.Amount,
            applyInfo.Remark,
            createId,
            modifyUserId,
            cancellationToken).ConfigureAwait(false);

        if (lockResult.Balance < 0M)
        {
            _ = await _cashServiceClient.RollbackTransferAsync(
                applyInfo.WalletId,
                applyInfo.Amount,
                applyInfo.Remark,
                createId,
                modifyUserId,
                cancellationToken).ConfigureAwait(false);

            throw new Exception("transfer fail");
        }

        var createInfo = new TransferBalanceCreateInfo(
            applyInfo.BrandId,
            applyInfo.Amount,
            "Player",
            applyInfo.PlayerCode,
            "Agent",
            applyInfo.TargetAgent.ToString(),
            applyInfo.IncludeInNetProfit);

        await _backOfficeRepository.InsertTransferBalanceFormAsync(
            createId,
            createInfo,
            modifyUserId,
            cancellationToken).ConfigureAwait(false);

        _ = await _agentWalletRepository.ExchangeAsync(
            applyInfo.BrandId,
            applyInfo.TargetAgentWalletId,
            applyInfo.TargetAgent,
            TransactionTypes.CompanyAdjust,
            applyInfo.Amount).ConfigureAwait(false);

        _ = await _cashServiceClient.CommitTransferAsync(
            applyInfo.WalletId,
            applyInfo.Amount,
            applyInfo.Remark,
            createId,
            modifyUserId,
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<TransactionResponse> AdjustAsync(
        PlayerAdjustmentTransactionType type,
        Guid walletId,
        decimal amount,
        string remark,
        Guid userId)
        => type switch
        {
            PlayerAdjustmentTransactionType.AgentAdjust => await _cashServiceClient
                .AdjustAgentV2Async(
                    walletId,
                    amount,
                    remark,
                    null,
                    userId)
                .ConfigureAwait(false),
            PlayerAdjustmentTransactionType.SystemAdjust => await _cashServiceClient
                .AdjustSystemV2Async(
                    walletId,
                    amount,
                    0M,
                    remark,
                    null,
                    userId)
                .ConfigureAwait(false),
            PlayerAdjustmentTransactionType.CompanyAdjust => await _cashServiceClient
                .AdjustCompanyV2Async(
                    walletId,
                    amount,
                    remark,
                    null,
                    userId)
                .ConfigureAwait(false),
            PlayerAdjustmentTransactionType.BonusAdjust => await _cashServiceClient
                .AdjustBonusV2Async(
                    walletId,
                    amount,
                    remark,
                    null,
                    userId)
                .ConfigureAwait(false),
            _ => throw new NotSupportedException()
        };
}
