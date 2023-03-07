using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandToken;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Queries.BrandToken;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public partial class BrandController : BaseContextController
{
    private readonly IMediator _mediator;

    public BrandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost(Name = "CreateBrand")]
    [Description("Create a Brand")]
    public Task<MessageResponse> CreateBrandAsync(CreateBrandRequest request)
        => _mediator.Send(request);

    [HttpGet("{id}", Name = "GetBrand")]
    [Description("Get a Brand")]
    public Task<GetBrandResponse> GetBrandAsync(Guid id)
        => _mediator.Send(new GetBrandRequest(id));

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

    [HttpGet(Name = "GetBrands")]
    [Description("Get brands: 品牌商列表")]
    public Task<PagedResponse<BrandModel>> GetBrandsAsync([FromQuery] GetBrandsRequest request)
        => _mediator.Send(request);

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

    [HttpGet("{id}/syncGames", Name = "SyncBrandGames")]
    [Description("sync brand games")]
    public Task<MessageResponse> SyncBrandGamesAsync(Guid id)
        => _mediator.Send(new SyncGamesRequest(id));

    [HttpPut("{id}", Name = "UpdateBrand")]
    [Description("Update a Brand")]
    public Task<MessageResponse> UpdateBrandAsync(Guid id, UpdateBrandRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }
}
