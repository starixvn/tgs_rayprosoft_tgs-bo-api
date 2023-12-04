using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/player/wallet")]
[ApiController]
public class PlayerWalletAdjustController(ILogger<PlayerWalletAdjustController> logger) : BaseContextController
{
    private readonly ILogger<PlayerWalletAdjustController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPut("locked/{playerCode}", Name = "AdjustPlayerLockedAmount")]
    public ValueTask AdjustLockedAmountAsync(
            [FromServices] ICommandService<AdjustPlayerLockedAmountCommand> adjustService,
            string playerCode,
            [FromBody] PlayerAdjustLockedAmountViewModel viewModel)
    {
        var command = new AdjustPlayerLockedAmountCommand(
            playerCode,
            viewModel.LockAmount,
            viewModel.Remark,
            Guid.Parse(UserId));

        return adjustService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Create a player adjustment to increase amount
    /// </summary>
    /// <param name="adjustService">The adjust service.</param>
    /// <param name="playerCode">The player code.</param>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [HttpPost("~/api/player/{playerCode}/adjust/increase")]
    [HttpPost("increase/{playerCode}", Name = "CreatePlayerIncreaseAmount")]
    public async Task<MessageResponse> CreatePlayerIncreaseAmountAsync(
    [FromServices] ICommandService<AdjustPlayerBalanceCommand> adjustService,
        string playerCode,
        CreatePlayerIncreaseAmountRequest request)
    {
        if (!Enum.IsDefined(request.TransactionType) ||
            !Enum.IsDefined((PlayerAdjustmentTransactionType)request.TransactionType))
            return new MessageResponse()
            {
                Valid = false,
                Message = "is not define type"
            };

        try
        {
            var command = new AdjustPlayerBalanceCommand(
                playerCode,
                AdjustmentType.Add,
                (PlayerAdjustmentTransactionType)request.TransactionType,
                request.Amount,
                request.BucketAmount,
                request.Remark,
                request.IncludeInNetProfit,
                request.BucketTypes,
                Guid.Parse(UserId),
                Username);

            await adjustService
                .ExecuteAsync(command, HttpContext.RequestAborted)
                .ConfigureAwait(false);

            return new MessageResponse()
            {
                Valid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Adjust Fail");

            return new MessageResponse
            {
                Valid = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Create a player adjustment to reduce amount
    /// </summary>
    /// <param name="adjustService">The adjust service.</param>
    /// <param name="transferService">The transfer service.</param>
    /// <param name="playerCode">The player code.</param>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [HttpPost("~/api/player/{playerCode}/adjust/reduce")]
    [HttpPost("reduce/{playerCode}", Name = "CreatePlayerReduceAmount")]
    public async Task<MessageResponse> CreatePlayerReduceAmountAsync(
        [FromServices] ICommandService<AdjustPlayerBalanceCommand> adjustService,
        [FromServices] ICommandService<PlayerTransferBalanceToParentAgentCommand> transferService,
        string playerCode,
        CreatePlayerReduceAmountRequest request)
    {
        if (!Enum.IsDefined(request.TransactionType) ||
            !Enum.IsDefined((PlayerAdjustmentTransactionType)request.TransactionType))
            return new MessageResponse()
            {
                Valid = false,
                Message = "is not define type"
            };

        try
        {
            if (request.TransactionType == PlayerReduceTransactionType.TransferAdjust)
            {
                var command = new PlayerTransferBalanceToParentAgentCommand(
                    playerCode,
                    request.Amount,
                    request.BucketAmount,
                    request.Remark,
                    request.IncludeInNetProfit,
                    Guid.Parse(UserId));

                await transferService
                    .ExecuteAsync(command, HttpContext.RequestAborted)
                    .ConfigureAwait(false);
            }
            else
            {
                var command = new AdjustPlayerBalanceCommand(
                    playerCode,
                    AdjustmentType.Subtract,
                    (PlayerAdjustmentTransactionType)request.TransactionType,
                    request.Amount,
                    request.BucketAmount,
                    request.Remark,
                    request.IncludeInNetProfit,
                    BucketTypes: Array.Empty<string>(),
                    Guid.Parse(UserId),
                    Username);

                await adjustService
                    .ExecuteAsync(command, HttpContext.RequestAborted)
                    .ConfigureAwait(false);
            }

            return new MessageResponse()
            {
                Valid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Adjust Fail");

            return new MessageResponse
            {
                Valid = false,
                Message = ex.Message
            };
        }
    }
}
