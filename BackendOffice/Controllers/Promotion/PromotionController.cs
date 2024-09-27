using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JsonCons.Utilities;
using Microsoft.AspNetCore.Mvc;
using Tgs.Paging;
using Tgs.Promotion;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using TGS.Services.BackOffice.Domain;
using PromotionStatus = TGS.Core.Models.Enums.PromotionStatus;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/[controller]")]
[ApiController]
public class PromotionController : BaseContextController
{
    [HttpPut("ChangeOrders", Name = "ChangeOrder")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    [Description("Change Promotion Display Order")]
    public async Task<MessageResponse> ChangeOrderAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        ChangeOrderRequest request)
    {
        var promotionOrders = request.Orders
            .Select(x => new PromotionOrder(x.Id, x.DisplayOrder))
            .ToArray();

        await promotionServiceClient.ChangePromotionOrdersAsync(
                promotionOrders,
                Username,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }

    [HttpPost(Name = "CreatePromotion")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    [Description("Create a promotion")]
    public async Task<MessageResponse> CreatePromotionAsync(
        [FromServices] IGetService<CreatePromotionMainInfoCommand, Guid> createMainInfoCommandService,
        [FromServices] ICommandService<UpdatePromotionContentCommand> updateContentCommandService,
        [FromServices] ICommandService<UpdatePromotionRuleCommand> updateRuleCommandService,
        [FromServices] ICommandService<UpdatePromotionStatusCommand> updateStatusCommandService,
        [FromBody] CreatePromotionRequest request)
    {
        var createMainInfoCommand = new CreatePromotionMainInfoCommand(
            request.BrandId,
            request.Name,
            request.PromotionType,
            request.From,
            request.To,
            request.Unlimited,
            request.TotalAmountLimit,
            request.AutoSentPlayerBonus,
            Username);

        var promotionId = await createMainInfoCommandService.GetAsync(
            createMainInfoCommand,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var updateContentCommand = new UpdatePromotionContentCommand(
            promotionId,
            request.SubSubject,
            request.Content,
            request.Localization ?? JsonDocument.Parse("{}"),
            request.DesktopBanner,
            request.MobileBanner,
            request.Terms,
            Username);

        await updateContentCommandService.ExecuteAsync(
            updateContentCommand,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var baseRule = JsonSerializer.SerializeToDocument(new
        {
            request.GameProviderIds,
            request.BonusAmountLimit,
            request.Percentage,
            request.TurnoverMultiple,
            request.GameType,
            request.VipLevels
        }, PromotionUtility.SerializerOptions);

        var rule = request.Rule is null
            ? baseRule
            : JsonMergePatch.ApplyMergePatch(baseRule.RootElement, request.Rule.RootElement);

        var updateRuleCommand = new UpdatePromotionRuleCommand(
            promotionId,
            rule,
            request.VipLevels,
            Username);

        await updateRuleCommandService.ExecuteAsync(
            updateRuleCommand,
            HttpContext.RequestAborted).ConfigureAwait(false);

        await updateStatusCommandService.ExecuteAsync(
            new UpdatePromotionStatusCommand(
                promotionId,
                request.Status switch
                {
                    SwitchStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                    SwitchStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                    _ => throw new NotImplementedException()
                },
                Username),
                HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPut("main/{brandId}", Name = "CreatePromotionMainInfo")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    public async ValueTask<ResponseViewModel<Guid?>> CreatePromotionMainInfoAsync(
        [FromServices] IGetService<CreatePromotionMainInfoCommand, Guid> getService,
        Guid brandId,
        CreatePromotionMainInfoViewModel viewModel)
    {
        var command = new CreatePromotionMainInfoCommand(
            brandId,
            viewModel.Name,
            viewModel.PromotionType,
            viewModel.From,
            viewModel.To,
            viewModel.Unlimited,
            viewModel.TotalAmountLimit,
            viewModel.AutoSentPlayerBonus,
            Username);

        var promotionId = await getService.GetAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new ResponseViewModel<Guid?>
        {
            Valid = true,
            Data = promotionId
        };
    }

    [HttpPut("{basePromotionId}:duplicate", Name = "DuplicatePromotion")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    [Description("duplicate a promotion")]
    public async Task<ResponseViewModel<Guid?>> DuplicatePromotionAsync(
        [FromServices] IGetService<DuplicatePromotionCommand, Guid> commandService,
        Guid basePromotionId)
    {
        var duplicatePromotionId = await commandService.GetAsync(
            new DuplicatePromotionCommand(
                basePromotionId,
                Username),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new ResponseViewModel<Guid?>
        {
            Valid = true,
            Data = duplicatePromotionId
        };
    }

    [HttpGet("content/{promotionId}", Name = "GetPromotionContent")]
    [BOAuthorize(["Promotion.PromotionList"])]
    public async ValueTask<PromotionContentViewModel> GetPromotionContentAsync(
        [FromServices] IGetService<GetPromotionContentQuery, PromotionContent> queryService,
        Guid promotionId)
    {
        var query = new GetPromotionContentQuery(promotionId);

        var content = await queryService.GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return content is null ? null : new PromotionContentViewModel
        {
            SubSubject = content.SubSubject,
            Content = content.Content,
            DesktopBanner = content.PersonalComputerBanner,
            MobileBanner = content.MobileBanner,
            Localization = content.Localization,
            Terms = content.Convention
        };
    }

    [HttpGet("main/{promotionId}", Name = "GetPromotionMainInfo")]
    [BOAuthorize(["Promotion.PromotionList"])]
    public async ValueTask<PromotionMainInfoViewModel> GetPromotionMainInfoAsync(
        [FromServices] IGetService<GetPromotionMainInfoQuery, PromotionMainInfo> queryService,
        Guid promotionId)
    {
        var query = new GetPromotionMainInfoQuery(promotionId);

        var mainInfo = await queryService.GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return mainInfo is null ? null : new PromotionMainInfoViewModel
        {
            Id = mainInfo.Id,
            BrandId = mainInfo.BrandId,
            Name = mainInfo.Name,
            From = mainInfo.EventDuring.From,
            To = mainInfo.EventDuring.To,
            PromotionType = mainInfo.PromotionType,
            Unlimited = mainInfo.PromotionLimitAmount is null,
            TotalAmountLimit = mainInfo.PromotionLimitAmount,
            AutoSentPlayerBonus = mainInfo.IsAutoSent
        };
    }

    [HttpGet("rule/{promotionId}", Name = "GetPromotionRule")]
    [BOAuthorize(["Promotion.PromotionList"])]
    public async ValueTask<JsonDocument> GetPromotionRuleAsync(
        [FromServices] IGetService<GetPromotionRuleQuery, PromotionRuleInfo> getService,
        Guid promotionId)
    {
        var query = new GetPromotionRuleQuery(promotionId);

        var rule = await getService.GetAsync(query).ConfigureAwait(false);

        if (rule is null)
            return null;

        var campaignDetail = JsonMergePatch.ApplyMergePatch(
            rule.CompaignDetail.RootElement,
            JsonSerializer.SerializeToElement(new
            {
                rule.VipLevels
            }, PromotionUtility.SerializerOptions));

        return campaignDetail;
    }

    [HttpGet(Name = "GetPromotions")]
    [BOAuthorize(["Promotion.PromotionList"])]
    [Description("Get promotions")]
    public async ValueTask<PagedResponse<PromotionListItemViewModel>> GetPromotionsAsync(
        [FromServices] IQueryService<PromotionListQuery, PromotionView> queryService,
        [FromQuery] PromotionListQueryViewModel viewModel,
        [FromQuery] PageableRequest pagingRequest)
    {
        var query = new PromotionListQuery(
            viewModel.BrandId,
            viewModel.Name,
            null,
            viewModel.Status,
            viewModel.PromotionTypes);

        if (viewModel.From.HasValue && viewModel.To.HasValue)
        {
            var offset = TimeSpan.FromHours(8D);
            var from = new DateTimeOffset(viewModel.From.Value.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;
            var to = new DateTimeOffset(viewModel.To.Value.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;

            query = query with
            {
                EventDuring = (from, to)
            };
        }

        var promotions = queryService.QueryAsync(query, HttpContext.RequestAborted);

        var totalCount = await promotions.CountAsync(HttpContext.RequestAborted).ConfigureAwait(false);

        var paging = promotions.CreatePaging()
            .MapProperty(nameof(PromotionListItemViewModel.Number), item => item.Order)
            .MapProperty(nameof(PromotionListItemViewModel.Name), item => item.Name)
            .MapProperty(nameof(PromotionListItemViewModel.Status), item => item.Status)
            .MapProperty(nameof(PromotionListItemViewModel.PromotionType), item => item.PromotionType);

        if (!string.IsNullOrWhiteSpace(pagingRequest.SortBy))
            paging.SortByPropertyName(pagingRequest.SortBy, pagingRequest.IsDesc);

        if (pagingRequest.ShowAll)
        {
            paging.Page = 1;
            paging.RowCount = totalCount;
        }
        else
        {
            paging.Page = pagingRequest.Page;
            paging.RowCount = pagingRequest.Rows;
        }

        promotions = promotions
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<PromotionListItemViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = await promotions.Select(p => new PromotionListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                PromotionType = p.PromotionType,
                AmountUsed = p.UsedAmount,
                From = p.EventDuring.From,
                To = p.EventDuring.To,
                Status = p.Status switch
                {
                    Tgs.Promotion.Clients.PromotionStatus.Enabled => SwitchStatus.Enabled,
                    Tgs.Promotion.Clients.PromotionStatus.Disabled => SwitchStatus.Disabled,
                    _ => throw new NotImplementedException()
                },
                TotalAmountLimit = p.PromotionLimitAmount,
                Unlimited = p.PromotionLimitAmount is null,
                VipLevels = p.VipLevels,
                Number = p.Order
            }).ToArrayAsync(HttpContext.RequestAborted).ConfigureAwait(false)
        };
    }

    [HttpGet("player", Name = "ListPlayerPromotions")]
    [BOAuthorize(["PlayerManagement.PlayerList.Update"])]
    public IAsyncEnumerable<PromotionViewModel> ListPlayerPromotionsAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromQuery] ListPlayerPromotionsRequest request)
    {
        var allowPlayerSelectPromotionTypes = new[] {
            "FirstDeposit",
            "DailySecondDeposit",
            "DailyFirstDeposit"
        };

        return promotionServiceClient
            .ListPromotionsAsync(request.BrandId, request.PlayerCode, HttpContext.RequestAborted)
            .Where(promotion => allowPlayerSelectPromotionTypes.Contains(promotion.PromotionType))
            .Select(promotion => new PromotionViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Localization = promotion.Localization
            });
    }

    [HttpPut("{promotionId}", Name = "UpdatePromotion")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    [Description("Update a promotion")]
    public async Task<MessageResponse> UpdatePromotionAsync(
        [FromServices] ICommandService<UpdatePromotionMainInfoCommand> updateMainInfoCommandService,
        [FromServices] ICommandService<UpdatePromotionContentCommand> updateContentCommandService,
        [FromServices] ICommandService<UpdatePromotionRuleCommand> updateRuleCommandService,
        [FromServices] ICommandService<UpdatePromotionStatusCommand> updateStatusCommandService,
        Guid promotionId,
        [FromBody] UpdatePromotionRequest request)
    {
        var updateMainInfoCommand = new UpdatePromotionMainInfoCommand(
            promotionId,
            request.Name,
            request.PromotionType,
            request.From,
            request.To,
            request.Unlimited,
            request.TotalAmountLimit,
            request.AutoSentPlayerBonus,
            Username);

        await updateMainInfoCommandService.ExecuteAsync(
            updateMainInfoCommand,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var updateContentCommand = new UpdatePromotionContentCommand(
            promotionId,
            request.SubSubject,
            request.Content,
            request.Localization ?? JsonDocument.Parse("{}"),
            request.DesktopBanner,
            request.MobileBanner,
            request.Terms,
            Username);

        await updateContentCommandService.ExecuteAsync(updateContentCommand, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var baseRule = JsonSerializer.SerializeToDocument(new
        {
            request.GameProviderIds,
            request.BonusAmountLimit,
            request.Percentage,
            request.TurnoverMultiple,
            request.GameType,
            request.VipLevels
        }, PromotionUtility.SerializerOptions);

        var rule = request.Rule is null
            ? baseRule
            : JsonMergePatch.ApplyMergePatch(baseRule.RootElement, request.Rule.RootElement);

        var updateRuleCommand = new UpdatePromotionRuleCommand(
            promotionId,
            rule,
            request.VipLevels,
            Username);

        await updateRuleCommandService.ExecuteAsync(updateRuleCommand, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        await updateStatusCommandService.ExecuteAsync(
            new UpdatePromotionStatusCommand(
                promotionId,
                request.Status switch
                {
                    SwitchStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                    SwitchStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                    _ => throw new NotImplementedException()
                },
                Username),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPatch("content/{promotionId}", Name = "UpdatePromotionContent")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    public async ValueTask<MessageResponse> UpdatePromotionContentAsync(
        [FromServices] ICommandService<UpdatePromotionContentCommand> commandService,
        Guid promotionId,
        UpdatePromotionContentViewModel viewModel)
    {
        var command = new UpdatePromotionContentCommand(
            promotionId,
            viewModel.SubSubject,
            viewModel.Content,
            viewModel.Localization ?? JsonDocument.Parse("{}"),
            viewModel.DesktopBanner,
            viewModel.MobileBanner,
            viewModel.Terms,
            Username);

        await commandService.ExecuteAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPatch("main/{promotionId}", Name = "UpdatePromotionMainInfo")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    public async ValueTask<MessageResponse> UpdatePromotionMainInfoAsync(
        [FromServices] ICommandService<UpdatePromotionMainInfoCommand> commandService,
        Guid promotionId,
        UpdatePromotionMainInfoViewModel viewModel)
    {
        var command = new UpdatePromotionMainInfoCommand(
            promotionId,
            viewModel.Name,
            viewModel.PromotionType,
            viewModel.From,
            viewModel.To,
            viewModel.Unlimited,
            viewModel.TotalAmountLimit,
            viewModel.AutoSentPlayerBonus,
            Username);

        await commandService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPatch("rule/{promotionId}", Name = "UpdatePromotionRule")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    public async ValueTask<MessageResponse> UpdatePromotionRuleAsync(
        [FromServices] ICommandService<UpdatePromotionRuleCommand> commandService,
        Guid promotionId,
        UpdatePromotionRuleViewModel viewModel)
    {
        var baseRule = JsonSerializer.SerializeToDocument(new
        {
            viewModel.GameProviderIds,
            viewModel.GameType,
            viewModel.VipLevels
        }, PromotionUtility.SerializerOptions);

        var rule = viewModel.Rule is null
            ? baseRule
            : JsonMergePatch.ApplyMergePatch(baseRule.RootElement, viewModel.Rule.RootElement);

        var command = new UpdatePromotionRuleCommand(
            promotionId,
            rule,
            viewModel.VipLevels,
            Username);

        await commandService.ExecuteAsync(command).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPut("{promotionId:guid}/{status}", Name = "UpdatePromotionStatus")]
    [BOAuthorize(["Promotion.PromotionList.Update"])]
    [Description("change a Promotion's status")]
    public async Task<MessageResponse> UpdatePromotionStatusAsync(
        [FromServices] ICommandService<UpdatePromotionStatusCommand> commandService,
        Guid promotionId,
        PromotionStatus status)
    {
        await commandService.ExecuteAsync(
            new UpdatePromotionStatusCommand(
                promotionId,
                status switch
                {
                    PromotionStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                    PromotionStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                    PromotionStatus.Archived => Tgs.Promotion.Clients.PromotionStatus.Archived,
                    _ => throw new NotImplementedException()
                },
                Username),
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }
}