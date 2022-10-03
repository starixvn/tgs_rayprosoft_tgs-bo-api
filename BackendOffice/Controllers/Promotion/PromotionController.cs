using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/[controller]")]
[ApiController]
public class PromotionController : BaseContextController
{
    private readonly IPromotionClient _client;

    public PromotionController(IPromotionClient client)
    {
        _client = client;
    }

    [HttpPut("ChangeOrders", Name = "ChangeOrder")]
    [Description("Change Promotion Display Order")]
    public async Task<MessageResponse> ChangeOrderAsync(ChangeOrderRequest request)
    {
        var promotionRequest = new Tgs.Promotion.Clients.Models.Promotion.ChangeOrderRequest
        {
            Orders = request.Orders.Select(x => new Tgs.Promotion.Clients.Models.Promotion.ChangeOrderModel
            {
                Id = x.Id,
                DisplayOrder = x.DisplayOrder
            }).ToArray()
        };

        var response = await _client.ChangeOrderAsync(promotionRequest);

        return new MessageResponse { Valid = response.Valid, Message = response.Message };
    }

    [HttpPost(Name = "CreatePromotion")]
    [Description("Create a promotion")]
    public async Task<MessageResponse> CreatePromotionAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromBody] CreatePromotionRequest request)
    {
        var createInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (request.From.UtcDateTime, request.To.UtcDateTime),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };
        var promotionId = await promotionServiceClient.CreatePromotionAsync(
            request.BrandId,
            createInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        await promotionServiceClient.SetPromotionVipLevelsAsync(
            promotionId,
            request.VipLevels,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var content = new PromotionContent
        {
            Content = request.Content,
            Convention = request.Terms,
            Localization = request.Localization,
            MobileBanner = request.MobileBanner,
            PersonalComputerBanner = request.DesktopBanner,
            SubSubject = request.SubSubject
        };
        await promotionServiceClient.SetPromotionContentAsync(
            promotionId,
            content,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (request.Rule == null)
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                new
                {
                    request.GameProviderIds,
                    request.BonusAmountLimit,
                    request.Percentage,
                    request.TurnoverMultiple,
                    request.GameType
                },
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
        else
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                request.Rule,
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }

        await promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            request.Status switch
            {
                SwitchStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                SwitchStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                _ => throw new NotImplementedException()
            },
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }

    [HttpGet("rule/{promotionId}", Name = "GetPromotionRule")]
    public async Task<JsonDocument> GetPromotionRuleAsync(
        [FromServices] IGetService<GetPromotionRuleQuery, PromotionRuleInfo> getService,
        Guid promotionId)
    {
        var query = new GetPromotionRuleQuery(promotionId);

        var rule = await getService.GetAsync(query).ConfigureAwait(false);

        return rule?.CompaignDetail;
    }

    [HttpGet(Name = "GetPromotions")]
    [Description("Get promotions")]
    public async Task<PagedResponse<PromotionModel>> GetPromotionsAsync([FromQuery] GetPromotionsRequest request)
    {
        var promotionRequest = new Tgs.Promotion.Clients.Models.Promotion.GetPromotionsRequest
        {
            BrandId = request.BrandId,
            Name = request.Name,
            From = request.From,
            To = request.To,
            Status = (short?)request.Status,
            Type = request.Type,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _client.GetPromotionsAsync(promotionRequest);

        return new PagedResponse<PromotionModel>
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new PromotionModel
            {
                AmountUsed = x.AmountUsed,
                AutoSentPlayerBonus = x.AutoSentPlayerBonus,
                BonusAmountLimit = x.BonusAmountLimit,
                BrandId = x.BrandId,
                Content = x.Content,
                CreatedByUsername = x.CreatedByUsername,
                CreatedOn = x.CreatedOn,
                DesktopBanner = x.DesktopBanner,
                From = x.From,
                GameProviderIds = x.GameProviderIds,
                GameType = (GameType?)x.GameType,
                Id = x.Id,
                Localization = x.Localization,
                MobileBanner = x.MobileBanner,
                Name = x.Name,
                Percentage = x.Percentage,
                PromotionType = x.PromotionType,
                Status = (SwitchStatus)x.Status,
                SubSubject = x.SubSubject,
                Terms = x.Terms,
                To = x.To,
                TotalAmountLimit = x.TotalAmountLimit,
                TurnoverMultiple = x.TurnoverMultiple,
                Unlimited = x.Unlimited,
                UpdatedByUsername = x.UpdatedByUsername,
                UpdatedOn = x.UpdatedOn,
                VipLevels = x.VipLevels,
                Number = x.Number
            }).ToArray()
        };
    }

    [HttpGet("player", Name = "ListPlayerPromotions")]
    public IAsyncEnumerable<PromotionViewModel> ListPlayerPromotionsAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
        [FromQuery] ListPlayerPromotionsRequest request)
    {
        var allowPlayerSelectPromotionTypes = new string[] {
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
    [Description("Update a promotion")]
    public async Task<MessageResponse> UpdatePromotionAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        Guid promotionId,
        [FromBody] UpdatePromotionRequest request)
    {
        var updateInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (request.From.UtcDateTime, request.To.UtcDateTime),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };

        await promotionServiceClient.UpdatePromotionMainInfoAsync(
            promotionId,
            updateInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        await promotionServiceClient.SetPromotionVipLevelsAsync(
            promotionId,
            request.VipLevels,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var content = new PromotionContent
        {
            Content = request.Content,
            Convention = request.Terms,
            Localization = request.Localization,
            MobileBanner = request.MobileBanner,
            PersonalComputerBanner = request.DesktopBanner,
            SubSubject = request.SubSubject
        };
        await promotionServiceClient.SetPromotionContentAsync(
            promotionId,
            content,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (request.Rule == null)
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                new
                {
                    request.GameProviderIds,
                    request.BonusAmountLimit,
                    request.Percentage,
                    request.TurnoverMultiple,
                    request.GameType
                },
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
        else
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                request.Rule,
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }

        await promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            request.Status switch
            {
                SwitchStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                SwitchStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                _ => throw new NotImplementedException()
            },
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }

    [HttpPut("{promotionId:guid}/{status}", Name = "UpdatePromotionStatus")]
    [Description("change a Promotion's status")]
    public async Task<MessageResponse> UpdatePromotionStatusAsync(Guid promotionId, Core.Models.Enums.PromotionStatus status)
    {
        var response = await _client.ChangePromotionStatusAsync(new(promotionId, (short)status));
        return new MessageResponse { Valid = response.Valid, Message = response.Message };
    }
}
