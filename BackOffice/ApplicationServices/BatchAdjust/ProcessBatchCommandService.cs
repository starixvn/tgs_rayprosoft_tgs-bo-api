using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;

internal class ProcessBatchCommandService : ICommandService<ProcessBatchCommand>
{
    private readonly IBatchTempRepository _batchTempRepository;
    private readonly ILogger<ProcessBatchCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerAdjustBalanceService _playerAdjustBalanceService;

    public ProcessBatchCommandService(
        IBatchTempRepository batchTempRepository,
        IPlayerAdjustBalanceService playerAdjustBalanceService,
        IMessageSender messageSender,
        ILogger<ProcessBatchCommandService> logger)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
        _playerAdjustBalanceService = playerAdjustBalanceService ?? throw new ArgumentNullException(nameof(playerAdjustBalanceService));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(ProcessBatchCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _batchTempRepository
            .BidBatchAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false))
            return;

        var clientToken = await _batchTempRepository.GetAdjustBalanceBatchClientTokenAsync(
            command.BatchId,
            cancellationToken).ConfigureAwait(false);

        var batch = await _batchTempRepository.GetAdjustBalanceBatchAsync(command.BatchId, cancellationToken)
            .Select(async item =>
            {
                try
                {
                    var applyInfo = new PlayerBalanceAdjustmentApplyInfo(
                        item.BrandId,
                        item.PlayerCode,
                        item.WalletId,
                        item.AdjustAmount >= 0M ? AdjustmentType.Add : AdjustmentType.Subtract,
                        command.AdjustTransferType,
                        AdjustTransferType.None,
                        Math.Abs(item.AdjustAmount),
                        item.Turnover,
                        command.IncludeInNetProfit,
                        item.Remark);

                    await _playerAdjustBalanceService.ProcessAdjustApplyAsync(
                        applyInfo,
                        command.ModifyUserId,
                        command.ModifyUserName,
                        cancellationToken).ConfigureAwait(false);

                    return new BatchAdjustBalanceItemResult(item.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Process batch item fail.");

                    return new BatchAdjustBalanceItemResult(item.Id, false, ex);
                }
            })
            .SelectMany(t => t.ToAsyncEnumerable())
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        await _batchTempRepository.SaveAdjustBalanceBatchResultAsync(
            command.BatchId,
            batch,
            cancellationToken).ConfigureAwait(false);

        await _batchTempRepository
            .CompleteBatchAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false);

        var notifyEvent = new NotifyEvent
        {
            Data = JsonSerializer.Serialize(
                new BatchAdjustBalanceNotifyBody(command.BatchId, clientToken),
                BackOfficeUtility.SerializerOptions)
        };

        notifyEvent.Tags.Add(new Tag
        {
            Name = "BrandId",
            Value = command.BrandId.ToString()
        });
        notifyEvent.Tags.Add(new Tag
        {
            Name = "UserId",
            Value = command.ModifyUserId.ToString()
        });
        notifyEvent.Tags.Add(new Tag
        {
            Name = "Feature",
            Value = "BatchAdjustBalance"
        });

        _logger.LogDebug("Notify begin publish.");

        await _messageSender.PublishAsync(
            "tgs.bo.notify",
            notifyEvent.ToByteArray(),
            cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Notify published. {evt}", notifyEvent);
    }
}
