using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Clients.Models.Promotion;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;
using TGS.Apis.BackendOffice.Extensions;
using TGS.Services.BackOffice.Application.Queries.Deposit;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Payment;

[Route("api/[controller]")]
[ApiController]
public class DepositController : BaseContextController
{
    [HttpGet("untreated/{brandId}")]
    public ValueTask<int> CountUntreatedDepositFormsAsync(
        [FromServices] IGetService<CountUntreatedDepositFormsQuery, int> countService,
        Guid brandId)
    {
        _ = (Activity.Current?.AddTag("username", Username));
        return countService.GetAsync(new CountUntreatedDepositFormsQuery(brandId));
    }

    [HttpGet("exportDeposit/{brandId}")]
    public async Task<IActionResult> ExportDepositAsync(
            [FromServices] IGetService<DepositFormQuery, DepositForms> queryService,
            [FromServices] IGetService<ExportDepositFormsCommand, MemoryStream> exportService,
            Guid brandId,
            [FromQuery] DepositFormRequest request,
            [FromQuery] PageableRequest paging)
    {
        var createOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
        var updateOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;

        if (request.BeginCreateOn != null && request.EndCreateOn != null)
            createOnRange = (request.BeginCreateOn.Value, request.EndCreateOn.Value);

        if (request.BeginUpdateOn != null && request.EndUpdateOn != null)
            updateOnRange = (request.BeginUpdateOn.Value, request.EndUpdateOn.Value);

        var query = new DepositFormQuery(
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
            createOnRange,
            updateOnRange,
            new Paging
            {
                IsDesc = paging.IsDesc,
                ShowAll = true,
                SortBy = paging.SortBy
            });

        var queryResponse = await queryService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var exportCommand = new ExportDepositFormsCommand(
            queryResponse.Items,
            queryResponse.Sum);

        return File(
            await exportService.GetAsync(exportCommand).ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Deposit.xlsx");
    }

    /// <summary>
    /// Gets the deposit form detail.
    /// </summary>
    /// <param name="getDepositFormService">The get deposit form service.</param>
    /// <param name="promotionClient">The promotion client.</param>
    /// <param name="brandId">The brand identifier.</param>
    /// <param name="formId">The form identifier.</param>
    /// <returns></returns>
    [HttpGet("{brandId}/{formId}")]
    public async Task<GenericResponse<DepositFormDetailViewModel>> GetDepositFormDetailAsync(
        [FromServices] IGetService<GetDepositFormDetailQuery, DepositFormInfo> getDepositFormService,
        [FromServices] IPromotionClient promotionClient,
        Guid brandId,
        Guid formId)
    {
        var formDetail = await getDepositFormService
            .GetAsync(new GetDepositFormDetailQuery(formId, brandId))
            .ConfigureAwait(false);

        var promotionNames = (await promotionClient
            .GetPromotionNamesAsync(new GetPromotionNamesRequest(
                new[] { formDetail.PromotionId }
                    .Where(id => id != null)
                    .Select(id => id.Value)
                    .ToArray()))
            .ConfigureAwait(false))
            .ToDictionary(model => model.Id, model => model.Name);

        var promotionBonus = (await promotionClient.GetPromotionBonusOfDepositAsync(
            new[] { formDetail.PromotionPlayerId }
                .Where(id => id != null)
                .Select(id => id.Value)
                .ToArray()).ConfigureAwait(false))
                .ToDictionary(model => model.Id, model => model.BonusAmount);

        return new GenericResponse<DepositFormDetailViewModel>
        {
            Valid = true,
            Data = new DepositFormDetailViewModel
            {
                Id = formDetail.Id,
                PlayerCode = formDetail.PlayerCode,
                Username = formDetail.UserName,
                BrandId = formDetail.BrandId,
                BrandPaymentId = formDetail.BrandPaymentId,
                PaymentMethodId = formDetail.PaymentMethodId,
                ChannelName = formDetail.ChannelName,
                Amount = formDetail.Amount,
                ActualAmount = formDetail.ActualAmount,
                Remark = formDetail.Remark,
                Status = formDetail.Status.ToFlowState(),
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
                PromotionName = formDetail.PromotionId != null
                    && promotionNames.TryGetValue(formDetail.PromotionId.Value, out var name)
                    ? name
                    : null,
                Bonus = formDetail.PromotionPlayerId != null
                    && promotionBonus.TryGetValue(formDetail.PromotionPlayerId.Value, out var bonus)
                    ? bonus
                    : null,
                HandlingFee = formDetail.HandlingFee,
            }
        };
    }

    /// <summary>
    /// Queries the deposit forms.
    /// </summary>
    /// <param name="queryService">The query service.</param>
    /// <param name="promotionClient">The promotion client.</param>
    /// <param name="agentClient">The agent client.</param>
    /// <param name="brandId">The brand identifier.</param>
    /// <param name="request">The request.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="playerServiceClient">The player client.</param>
    /// <returns></returns>
    [HttpGet("{brandId}")]
    public async Task<DepositViewModel> QueryDepositFormsAsync(
        [FromServices] IGetService<DepositFormQuery, DepositForms> queryService,
        [FromServices] IPromotionClient promotionClient,
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] IAgentClient agentClient,
        Guid brandId,
        [FromQuery] DepositFormRequest request,
        [FromQuery] PageableRequest paging)
    {
        var createOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
        var updateOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;

        if (request.BeginCreateOn != null && request.EndCreateOn != null)
            createOnRange = (request.BeginCreateOn.Value, request.EndCreateOn.Value);

        if (request.BeginUpdateOn != null && request.EndUpdateOn != null)
            updateOnRange = (request.BeginUpdateOn.Value, request.EndUpdateOn.Value);

        var query = new DepositFormQuery(
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

        var result = await queryService.GetAsync(query).ConfigureAwait(false);

        var promotionNames = (await promotionClient
                .GetPromotionNamesAsync(new GetPromotionNamesRequest(
                    result.Items
                        .Where(info => info.PromotionId != null)
                        .Select(info => info.PromotionId.Value)
                        .ToArray()))
                .ConfigureAwait(false))
            .ToDictionary(model => model.Id, model => model.Name);

        var promotionBonus = (await promotionClient.GetPromotionBonusOfDepositAsync(
                result.Items
                    .Where(info => info.PromotionPlayerId != null)
                    .Select(info => info.PromotionPlayerId.Value)
                    .ToArray()).ConfigureAwait(false))
            .ToDictionary(model => model.Id, model => model.BonusAmount);

        var playerInfos = playerServiceClient
            .QueryPlayerInfoListAsync(brandId, result.Items.Select(x => x.PlayerCode),
                HttpContext.RequestAborted);

        var agents = agentClient.GetAgentNamesAsync(brandId);

        var players = playerInfos.SelectAwait(async x =>
            {
                var agentInfo = await agents.FirstOrDefaultAsync(a => a.AgentId == x.AgentId);

                return (x.PlayerCode, agentInfo?.Username);
            }).ToEnumerable();

        var response = new DepositViewModel
        {
            Valid = true,
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(form => new DepositFormViewModel
                {
                    Id = form.Id,
                    PlayerCode = form.PlayerCode,
                    Username = form.UserName,
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
                    ReviewedUserName = form.ReviewedUserName,
                    OrderNo = form.OrderNo,
                    PromotionName = form.PromotionId != null
                                    && promotionNames.TryGetValue(form.PromotionId.Value, out var name)
                        ? name
                        : null,
                    Bonus = form.PromotionPlayerId != null
                            && promotionBonus.TryGetValue(form.PromotionPlayerId.Value, out var bonus)
                        ? bonus
                        : null,
                    HandlingFee = form.HandlingFee,
                    AgentName = players.FirstOrDefault(x => x.PlayerCode == form.PlayerCode).Username
                })
                .ToArray(),
            Sum = result.Sum
        };

        return response;
    }

    /// <summary>
    /// Reviews the deposit form.
    /// </summary>
    /// <param name="reviewCommandService">The review command service.</param>
    /// <param name="formId">The form identifier.</param>
    /// <param name="reviewResponse">The review responses.</param>
    [HttpPost("{formId}")]
    public async Task<MessageResponse> ReviewDepositFormAsync(
        [FromServices] ICommandService<ReviewDepositFormCommand> reviewCommandService,
        Guid formId,
        [FromBody] DepositReviewRequest reviewResponse)
    {
        var command = new ReviewDepositFormCommand(
            reviewResponse.BrandId,
            formId,
            reviewResponse.ReviewResponse.ToReviewState(),
            reviewResponse.ActualAmount,
            reviewResponse.Remark,
            UserId,
            Username);

        await reviewCommandService
            .ExecuteAsync(command)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }
}