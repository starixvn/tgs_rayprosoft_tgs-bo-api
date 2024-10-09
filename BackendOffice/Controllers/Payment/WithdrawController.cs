using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using Tgs.Agent.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;
using TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;
using TGS.Apis.BackendOffice.Extensions;
using TGS.Apis.BackendOffice.Filters;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using TGS.Services.BackOffice.Application.Queries.Withdraw;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;
using UserType = Tgs.Payment.Clients.UserType;

namespace TGS.Apis.BackendOffice.Controllers.Payment;

[Route("api/[controller]")]
[ApiController]
public class WithdrawController : BaseContextController
{
    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("untreated/{brandId}")]
    public ValueTask<int> CountUntreatedWithdrawFormsAsync(
        [FromServices] IGetService<CountUntreatedWithdrawFormsQuery, int> countService,
        Guid brandId)
    {
        _ = (Activity.Current?.AddTag("username", Username));
        return countService.GetAsync(new CountUntreatedWithdrawFormsQuery(brandId));
    }

    [BOTypeFilter(typeof(MerchanPermissionBodyFiler), Arguments = ["brand", "BrandId"])]
    [HttpPost("player", Name = "CreatePlayerWithdrawRequest")]
    public async Task<MessageResponse> CreatePlayerWithdrawRequestAsync(
        [FromServices] IGetService<CheckPlayerIdentityCommand, bool> checkService,
        [FromServices] IGetService<CreateWithdrawFormCommand, bool> checkCanCreateService,
        [FromServices] ICommandService<CreateWithdrawFormCommand> createFormService,
        [FromBody] CreateWithdrawRequest request)
    {
        var checkCommand = new CheckPlayerIdentityCommand(request.BrandId, request.PlayerCode);

        var playerCanWithdraw = await checkService
            .GetAsync(checkCommand, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (playerCanWithdraw is false)
            return CreateInvalidResponse<MessageResponse>(Player.PlayerNotFoundOrIdentityAuthFailed);

        var command = new CreateWithdrawFormCommand(
            request.PlayerCode,
            request.BrandId,
            request.BankInfoId,
            request.Amount,
            request.Note);

        if (!await checkCanCreateService
                .GetAsync(command, HttpContext.RequestAborted)
                .ConfigureAwait(false))
            return CreateInvalidResponse<MessageResponse>(Player.WithdrawAmountOrBrandPaymentIsWrong);

        try
        {
            await createFormService
                .ExecuteAsync(command, HttpContext.RequestAborted)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            if (ex.Message == "Can't find player.")
                return CreateInvalidResponse<MessageResponse>(Player.WithdrawAmountOrBrandPaymentIsWrong);
            else if (ex.Message == "InsufficientBucket")
                return CreateInvalidResponse<MessageResponse>(Player.InsufficientBucket);
            else if (ex.Message == "InsufficientFunds")
                return CreateInvalidResponse<MessageResponse>(Player.InsufficientFunds);
            else
                throw;
        }

        return CreateValidResponse<MessageResponse>();
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("exportWithdraw/{brandId}")]
    public async Task<IActionResult> ExportWithdrawAsync(
        [FromServices] IGetService<WithdrawFormQuery, WithdrawForms> queryService,
        [FromServices] IGetService<ExportWithdrawFormsCommand, MemoryStream> exportService,
        Guid brandId,
        [FromQuery] WithdrawFormRequest request,
        [FromQuery] PageableRequest paging)
    {
        var createOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
        var updateOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;

        if (request.BeginCreateOn != null && request.EndCreateOn != null)
            createOnRange = (request.BeginCreateOn.Value, request.EndCreateOn.Value);

        if (request.BeginUpdateOn != null && request.EndUpdateOn != null)
            updateOnRange = (request.BeginUpdateOn.Value, request.EndUpdateOn.Value);

        var query = new WithdrawFormQuery(
            brandId,
            request.Username,
            request.ChannelName,
            request.ThirdPartyOrderNo,
            request.OrderNo,
            request.FormId,
            request.PaymentId,
            request.PaymentMethodId,
            request.Status?.SelectMany(s => s.ToPaymentFlowState()).Distinct()
                ?? default(FlowState?).ToPaymentFlowState(),
            request.BrandPaymentIds.Distinct(),
            request.UserType,
            createOnRange,
            updateOnRange,
            new Paging
            {
                IsDesc = paging.IsDesc,
                ShowAll = true,
                SortBy = paging.SortBy
            });

        var queryResponse = await queryService.GetAsync(query, HttpContext.RequestAborted).ConfigureAwait(false);

        var exportCommand = new ExportWithdrawFormsCommand(
            queryResponse.Items,
            queryResponse.Sum);

        return File(
            await exportService.GetAsync(exportCommand).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Withdraw.xlsx");
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("{brandId}/{formId}")]
    public async Task<GenericResponse<WithdrawFormDetailViewModel>> GetWithdrawFormDetailAsync(
        [FromServices] IGetService<GetWithdrawFormDetailQuery, WithdrawFormInfo> getWithdrawFormService,
        [FromServices] IGetService<GetWithdrawFormRealNameQuery, string> getWithdrawRealNameService,
        Guid brandId,
        Guid formId)
    {
        var formDetail = await getWithdrawFormService
            .GetAsync(new GetWithdrawFormDetailQuery(formId, brandId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var realName = await getWithdrawRealNameService
            .GetAsync(new GetWithdrawFormRealNameQuery(formDetail.UserType, formDetail.UserId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new GenericResponse<WithdrawFormDetailViewModel>
        {
            Valid = true,
            Data = new WithdrawFormDetailViewModel
            {
                Id = formDetail.Id,
                UserId = formDetail.UserId,
                Username = formDetail.UserName,
                UserType = formDetail.UserType.ToString(),
                BrandId = formDetail.BrandId,
                BrandPaymentId = formDetail.BrandPaymentId,
                PaymentMethodId = formDetail.PaymentMethodId,
                ChannelName = formDetail.ChannelName,
                Amount = formDetail.Amount,
                ActualAmount = formDetail.ActualAmount,
                Remark = formDetail.Remark,
                PlayerRemark = formDetail.PlayerRemark,
                Status = formDetail.Status.ToFlowState(),
                BankInfo = string.IsNullOrWhiteSpace(formDetail.BankInfo)
                    ? null
                    : JsonDocument.Parse(formDetail.BankInfo),
                ApplyInfo = string.IsNullOrWhiteSpace(formDetail.ApplyInfo)
                    ? null
                    : JsonDocument.Parse(formDetail.ApplyInfo),
                PaymentReturn = string.IsNullOrWhiteSpace(formDetail.PaymentReturn)
                    ? null
                    : JsonDocument.Parse(formDetail.PaymentReturn),
                CreatedOn = formDetail.CreatedOn,
                UpdatedOn = formDetail.UpdatedOn,
                ThirdPartyOrderNo = formDetail.ThirdPartyOrderNo,
                OrderNo = formDetail.OrderNo,
                ReviewedOn = formDetail.ReviewedOn,
                ReviewedUsername = formDetail.ReviewedUserName,
                PaidOn = formDetail.PaidOn,
                PaidUserId = formDetail.PaidUserId,
                PaidUsername = formDetail.PaidUserName,
                Note = formDetail.Note,
                HandlingFee = formDetail.HandlingFee,
                RealName = realName,
            }
        };
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("{brandId}")]
    public async Task<PagingWithdrawFormViewModel> QueryWithdrawFormsAsync(
        [FromServices] IGetService<WithdrawFormQuery, WithdrawForms> queryService,
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] IAgentClient agentClient,
        Guid brandId,
        [FromQuery] WithdrawFormRequest request,
        [FromQuery] PageableRequest paging)
    {
        var createOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
        var updateOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;

        if (request.BeginCreateOn != null && request.EndCreateOn != null)
            createOnRange = (request.BeginCreateOn.Value, request.EndCreateOn.Value);

        if (request.BeginUpdateOn != null && request.EndUpdateOn != null)
            updateOnRange = (request.BeginUpdateOn.Value, request.EndUpdateOn.Value);

        var query = new WithdrawFormQuery(
            brandId,
            request.Username,
            request.ChannelName,
            request.ThirdPartyOrderNo,
            request.OrderNo,
            request.FormId,
            request.PaymentId,
            request.PaymentMethodId,
            request.Status?.SelectMany(s => s.ToPaymentFlowState()).Distinct()
                ?? default(FlowState?).ToPaymentFlowState(),
            request.BrandPaymentIds.Distinct(),
            request.UserType,
            createOnRange,
            updateOnRange,
            new Paging
            {
                IsDesc = paging.IsDesc,
                Page = paging.Page,
                Rows = paging.Rows,
                ShowAll = paging.ShowAll,
                SortBy = paging.SortBy
            });

        var result = await queryService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerInfos = playerServiceClient.QueryPlayerInfoListAsync(brandId,
            result.Items.Where(x => x.UserType == UserType.Player).Select(x => x.UserId),
            HttpContext.RequestAborted);

        var agents = agentClient.GetAgentNamesAsync(brandId);

        var players = playerInfos.SelectAwait(async x =>
        {
            var agentInfo = await agents.FirstOrDefaultAsync(a => a.AgentId == x.AgentId);

            return (x.PlayerCode, agentInfo?.Username);
        }).ToEnumerable();

        var response = new PagingWithdrawFormViewModel()
        {
            Valid = true,
            TotalCount = result.TotalCount,
            Items = result.Items
            .Select(form => new WithdrawFormViewModel
            {
                Id = form.Id,
                UserId = form.UserId,
                Username = form.UserName,
                UserType = form.UserType.ToString(),
                BrandId = form.BrandId,
                BrandPaymentId = form.BrandPaymentId,
                PaymentMethodId = form.PaymentMethodId,
                ChannelName = form.ChannelName,
                Status = form.Status.ToFlowState(),
                Amount = form.Amount,
                ActualAmount = form.ActualAmount,
                Remark = form.Remark,
                CreatedOn = form.CreatedOn,
                UpdatedOn = form.UpdatedOn,
                ThirdPartyOrderNo = form.ThirdPartyOrderNo,
                ReviewedUsername = form.ReviewedUserName,
                ReviewedOn = form.ReviewedOn,
                OrderNo = form.OrderNo,
                PaidOn = form.PaidOn,
                PaidUserId = form.PaidUserId,
                PaidUsername = form.PaidUserName,
                AgentName = players.FirstOrDefault(x => x.PlayerCode == form.UserId).Username
            })
            .ToArray(),
            Sum = result.Sum
        };

        return response;
    }

    [BOTypeFilter(typeof(MerchanPermissionBodyFiler), Arguments = ["brand", "BrandId"])]
    [HttpPost("{formId}")]
    public async Task<MessageResponse> ReviewWithdrawFormAsync(
        [FromServices] ICommandService<ReviewWithdrawFormCommand> reviewCommandService,
        Guid formId,
        [FromBody] WithdrawReviewRequest request)
    {
        var command = new ReviewWithdrawFormCommand(
            request.BrandId,
            formId,
            request.BrandPaymentId,
            request.ReviewResponse.ToReviewState(),
            request.Remark,
            request.PlayerRemark,
            UserId,
            Username);

        await reviewCommandService
            .ExecuteAsync(command)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }
}