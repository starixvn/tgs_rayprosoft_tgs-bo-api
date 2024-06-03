using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandToken;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Queries.BrandToken;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;
using Valhalla.MessageQueue;
using BrandListQuery = TGS.Services.BackOffice.ApplicationServices.BrandListQuery;
using Resource = TGS.Services.BackOffice.Application.Resources;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public partial class BrandController(IMediator mediator) : BaseContextController
{
    [HttpPost(Name = "CreateBrand")]
    [Description("Create a Brand")]
    public async ValueTask<MessageResponse> CreateBrandAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IGetService<CreateBrandCommand, Guid> createBrandCommandService,
        [FromServices] IMessageSender messageSender,
        CreateBrandRequest request)
    {
        var brands = await brandRelationQueryService
            .QueryAsync(new BrandRelationQuery(), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (brands.Any(x => x.BrandName == request.Name))
            return new MessageResponse
            {
                Valid = false,
                Message = Resource.Brand.NameExisted
            };

        if (brands.Any(x => x.BrandCode.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            return new MessageResponse
            {
                Valid = false,
                Message = Resource.Brand.CodeExisted
            };

        var createBrandCommand = new CreateBrandCommand(
            request.Code,
            request.Name,
            request.Description,
            request.OperationalEmail,
            request.TimeSpanMinutes,
            request.EnablePlayerIdentity,
            request.LicenseeId,
            request.CurrencyCodes,
            request.Languages,
            request.SettlementIntervalType,
            request.SettlementDay,
            request.GameProviders,
            request.AgentDividendType,
            request.CheckDividendActiveCount,
            request.AutoClearWageringRequirement,
            request.AutoSendAgentBonus,
            request.EnablePlayerAgent,
            request.AutoClearWageringRequirementBalance,
            request.DepositHandlingFeeRateTenPercentile,
            request.WithdrawHandlingFeeRateTenPercentile,
            request.Domains,
            request.BetLimits,
            Username);

        _ = await createBrandCommandService
            .GetAsync(createBrandCommand, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        await messageSender.PublishAsync(
            "tgs.bo.cache.brand-relation.clear",
            new Empty().ToByteArray(),
            HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpGet("{id}", Name = "GetBrand")]
    [Description("Get a Brand")]
    public Task<GetBrandResponse> GetBrandAsync(Guid id)
        => mediator.Send(new GetBrandRequest(id), HttpContext.RequestAborted);

    [HttpGet("logo/{brandId}", Name = "GetBrandLogoInfo")]
    public async ValueTask<ResponseViewModel<BrandLogoInfoViewModel>> GetBrandLogoInfoAsync(
        [FromServices] IGetService<GetBrandLogoQuery, BrandLogoInfo> getService,
        Guid brandId)
    {
        var query = new GetBrandLogoQuery(brandId);

        var info = await getService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return info == null
            ? new ResponseViewModel<BrandLogoInfoViewModel>
            {
                Valid = true,
                Data = new BrandLogoInfoViewModel
                {
                    BrandId = brandId
                }
            }
            : new ResponseViewModel<BrandLogoInfoViewModel>
            {
                Valid = true,
                Data = new BrandLogoInfoViewModel
                {
                    BrandId = info.BrandId,
                    DesktopLogoUrl = info.DesktopLogoUrl,
                    LoadingIconUrl = info.LoadingUrl,
                    MobileLogoUrl = info.MobileLogoUrl
                }
            };
    }

    [HttpGet("token/{brandId}", Name = "GetBrandToken")]
    [Description("Get a brand's token")]
    public async Task<GetBrandTokenResponse> GetBrandTokenAsync(
        Guid brandId,
        [FromServices] IGetService<BrandTokenQuery, string> getService)
    {
        var query = new BrandTokenQuery(brandId);
        var token = await getService
            .GetAsync(query)
            .ConfigureAwait(false);

        return new GetBrandTokenResponse()
        {
            Token = token
        };
    }

    [HttpPut("logo", Name = "ModifyBrandLogoInfo")]
    public ValueTask ModifyBrandLogoInfoAsync(
        [FromServices] ICommandService<ModifyBrandLogoInfoCommand> modifyService,
        BrandLogoInfoViewModel request)
    {
        var command = new ModifyBrandLogoInfoCommand(
            request.BrandId,
            request.DesktopLogoUrl,
            request.MobileLogoUrl,
            request.LoadingIconUrl,
            Username);

        return modifyService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpGet(Name = "QueryBrandList")]
    [Description("Query brands: 品牌商列表")]
    public async ValueTask<PagedResponse<BrandListItemViewModel>> QueryBrandListAsync(
        [FromServices] IQueryService<BrandListQuery, BrandListItem> brandListQueryService,
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromQuery] BrandListQueryViewModel viewModel,
        [FromQuery] PageableRequest pagingRequest)
    {
        var brandRelations = await brandRelationQueryService
            .QueryAsync(new BrandRelationQuery(), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var brandIds = new HashSet<Guid>();

        if (viewModel.ResellerId.HasValue)
        {
            var ids = brandRelations
                .Where(b => b.ResellerId == viewModel.ResellerId.Value)
                .Select(b => b.BrandId);

            brandIds.UnionWith(ids);
        }

        if (viewModel.LicenseeId.HasValue)
        {
            var ids = brandRelations
                .Where(b => b.LicenseeId == viewModel.LicenseeId.Value)
                .Select(b => b.BrandId);

            brandIds.UnionWith(ids);
        }

        if (viewModel.BrandId.HasValue)
            _ = brandIds.Add(viewModel.BrandId.Value);

        var query = new BrandListQuery(
            brandIds,
            viewModel.Keyword,
            viewModel.Status);

        var items = brandListQueryService
            .QueryAsync(query, HttpContext.RequestAborted);

        if (viewModel.Status is null)
            items = items.Where(b => b.Status is BrandStatus.Active or BrandStatus.Maintenance);

        var brands = items.Select(item =>
        {
            var brand = brandRelations.First(b => b.BrandId == item.BrandId);

            return new
            {
                item.BrandId,
                brand.BrandCode,
                brand.BrandName,
                brand.LicenseeId,
                brand.LicenseeName,
                brand.ResellerId,
                brand.ResellerName,
                Status = item.Status switch
                {
                    BrandStatus.Active => Core.Models.Enums.BrandStatus.Active,
                    BrandStatus.Archived => Core.Models.Enums.BrandStatus.Archived,
                    BrandStatus.Inactive => Core.Models.Enums.BrandStatus.Inactive,
                    BrandStatus.Maintenance => Core.Models.Enums.BrandStatus.Maintenance,
                    _ => throw new NotImplementedException()
                },
                item.Description,
                item.OperationalEmail,
                item.CreatedOn,
                item.UpdatedOn
            };
        });

        var totalCount = await brands.CountAsync().ConfigureAwait(false);

        var paging = brands.CreatePaging()
            .MapProperty(nameof(BrandListItemViewModel.Code), item => item.BrandCode)
            .MapProperty(nameof(BrandListItemViewModel.Name), item => item.BrandName)
            .MapProperty(nameof(BrandListItemViewModel.Status), item => item.Status)
            .MapProperty(nameof(BrandListItemViewModel.CreatedOn), item => item.CreatedOn)
            .MapProperty(nameof(BrandListItemViewModel.UpdatedOn), item => item.UpdatedOn);

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

        brands = brands
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<BrandListItemViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = await brands.Select(item => new BrandListItemViewModel
            {
                Id = item.BrandId,
                Code = item.BrandCode,
                Name = item.BrandName,
                LicenseeId = item.LicenseeId,
                LicenseeName = item.LicenseeName,
                ResellerId = item.ResellerId,
                ResellerName = item.ResellerName,
                Status = item.Status,
                Description = item.Description,
                OperationalEmail = item.OperationalEmail,
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn
            }).ToArrayAsync().ConfigureAwait(false)
        };
    }

    [HttpGet("{id}/syncGames", Name = "SyncBrandGames")]
    [Description("sync brand games")]
    public Task<MessageResponse> SyncBrandGamesAsync(Guid id)
        => mediator.Send(new SyncGamesRequest(id));

    [HttpPut("{id}", Name = "UpdateBrand")]
    [Description("Update a Brand")]
    public Task<MessageResponse> UpdateBrandAsync(Guid id, UpdateBrandRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }

    [HttpGet("platform-account-fund/{brandId:Guid}", Name = "GetBrandPlatformAccountFundFlag")]
    public async ValueTask<bool> GetBrandPlatformAccountFundFlagAsync(
        [FromServices] IBrandServiceClient brandServiceClient,
        Guid brandId)
    {
        var flag = await brandServiceClient.GetBrandPlatformAccountFundAsync(
            brandId,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return flag ?? false;
    }

    [HttpPatch("platform-account-fund/{brandId:Guid}", Name = "SetBrandPlatformAccountFundFlag")]
    public ValueTask SetBrandPlatformAccountFundFlagAsync(
        [FromServices] IBrandServiceClient brandServiceClient,
        Guid brandId,
        [FromBody] bool enabled)
        => brandServiceClient.SetBrandPlatformAccountFundAsync(
            brandId,
            enabled,
            Username,
            HttpContext.RequestAborted);

    [HttpPatch("enable-sms/{brandId:Guid}", Name = "SetBrandEnableSms")]
    public ValueTask SetBrandEnableSmsAsync(
        [FromServices] ICommandService<SetBrandEnableSmsCommand> commandService,
        Guid brandId,
        [FromBody] bool enableSms)
        => commandService.ExecuteAsync(new SetBrandEnableSmsCommand(
            brandId,
            enableSms,
            Username),
            HttpContext.RequestAborted);
}