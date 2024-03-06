using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.Application.Commands.PlayerBank;
using TGS.Services.BackOffice.Application.Models.PlayerBank;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/player")]
[ApiController]
public class PlayerBankInfoController : BaseContextController
{
    [HttpPost("{playerCode}/bank", Name = "CreatePlayerBankInfo")]
    [Description("Create a player bank info")]
    public async Task<MessageResponse> CreatePlayerBankAsync(
        [FromServices] ICommandService<CreatePlayerBankInfoCommand> commandService,
        string playerCode,
        [FromBody] CreatePlayerBankRequest request)
    {
        var command = new CreatePlayerBankInfoCommand(
            playerCode,
            request.BrandId,
            request.PaymentChannelId,
            request.BankType,
            request.Detail,
            Username);

        await commandService.ExecuteAsync(command, Request.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [Route("{playerCode}/bank/{bankInfoId}", Name = "DeletePlayerBankInfo")]
    [HttpDelete]
    [Description("Delete a player bank info")]
    public async Task<MessageResponse> DeletePlayerBankAsync(
        [FromServices] ICommandService<DeletePlayerBankInfoCommand> commandService,
        string playerCode,
        Guid bankInfoId)
    {
        var command = new DeletePlayerBankInfoCommand(playerCode, bankInfoId);

        await commandService.ExecuteAsync(command, Request.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [Route("{playerCode}/bank/{bankInfoId}/default", Name = "SetDefaultPlayerBankInfo")]
    [HttpPut]
    [Description("Set a player default bank info")]
    public async Task<MessageResponse> SetDefaultPlayerBankAsync(
        [FromServices] ICommandService<SetDefaultPlayerBankInfoCommand> commandService,
        string playerCode,
        Guid bankInfoId)
    {
        var command = new SetDefaultPlayerBankInfoCommand(playerCode, bankInfoId, Username);

        await commandService.ExecuteAsync(command, Request.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("{playerCode}/bank/{bankInfoId}", Name = "UpdatePlayerBankInfo")]
    [Description("Update player bank")]
    public async Task<MessageResponse> UpdatePlayerBankAsync(
        [FromServices] ICommandService<UpdatePlayerBankInfoCommand> commandService,
        string playerCode,
        Guid bankInfoId,
        [FromBody] UpdatePlayerBankRequest request)
    {
        var command = new UpdatePlayerBankInfoCommand(
            playerCode,
            bankInfoId,
            request.BrandId,
            request.PaymentChannelId,
            request.BankType,
            request.Detail,
            Username);

        await commandService.ExecuteAsync(command, Request.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet("{playerCode}/bank", Name = "ListPlayerBankInfos")]
    public async Task<PagedResponse<PlayerBankInfoViewModel>> ListPlayerBankInfosAsync(
        [FromServices] IPaymentServiceClient paymentServiceClient,
        string playerCode)
    {
        var items = await paymentServiceClient.GetPlayerBankInfosAsync(playerCode, HttpContext.RequestAborted)
            .Select(bankInfo => new PlayerBankInfoViewModel
            {
                Id = bankInfo.Id,
                PaymentChannelId = bankInfo.PaymentChannelId,
                BankType = bankInfo.BankType,
                Detail = bankInfo.Detail,
                IsDefault = bankInfo.IsDefault,
                UpdatedOn = bankInfo.UpdatedOn,
                Modifier = bankInfo.Modifier
            })
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<PlayerBankInfoViewModel>
        {
            Valid = true,
            TotalCount = items.Length,
            Items = items
        };
    }
}
