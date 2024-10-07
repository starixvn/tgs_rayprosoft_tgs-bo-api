using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/brand/player-contract")]
[ApiController]
public class BrandPlayerContractController : BaseContextController
{
    [HttpGet(Name = "ListPlayerContractType")]
    public IAsyncEnumerable<PlayerPropertyName> ListPlayerContractTypeAsync()
    {
        var contractType = new[]
        {
            PlayerPropertyName.Line,
            PlayerPropertyName.Viber,
            PlayerPropertyName.Telegram,
            PlayerPropertyName.FacebookMessenger,
            PlayerPropertyName.WhatsApp,
            PlayerPropertyName.Qq,
            PlayerPropertyName.Wechat,
            PlayerPropertyName.Zalo
        };

        return contractType.ToAsyncEnumerable();
    }

    [HttpGet("{brandId:guid}/display", Name = "ListBrandPlayerContractDisplayType")]
    public async IAsyncEnumerable<BrandPlayerContractDisplayViewModel> ListBrandPlayerContractDisplayTypeAsync(
        [FromServices] IQueryService<BrandPlayerContractDisplayQuery, BrandPlayerContractDisplayInfo> queryService,
        Guid brandId)
    {
        var info = await queryService
            .QueryAsync(new BrandPlayerContractDisplayQuery(brandId), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        foreach (var item in info)
        {
            yield return new BrandPlayerContractDisplayViewModel
            {
                Name = item.Name,
                IsVisualized = item.IsVisualized,
            };
        }
    }

    [HttpPatch("{brandId:guid}", Name = "UpsertBrandPlayerContractDisplay")]
    public ValueTask UpsertBrandPlayerContractDisplayAsync(
        [FromServices] ICommandService<UpsertBrandPlayerContractDisplayCommand> commandService,
        Guid brandId,
        [FromBody] UpsertBrandPlayerContractRequest request)
    {
        return commandService.ExecuteAsync(
            new UpsertBrandPlayerContractDisplayCommand(brandId, request, Username),
            HttpContext.RequestAborted);
    }
}