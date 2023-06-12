using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/adjust-wallet")]
[ApiController]
public class AdjustWalletController : ControllerBase
{
    [HttpGet(Name = "QueryAdjustSummaries")]
    public IAsyncEnumerable<AdjustSummaryListItemViewModel> QueryAdjustSummariesAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<AdjustSummariesQuery, AdjustSummaryListItem> queryService,
        [FromQuery] AdjustSummaryQueryViewModel queryViewModel)
        => brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
            .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
            .AsTask()
            .ToAsyncEnumerable()
            .SelectMany(dict => queryService
                .QueryAsync(
                    new((queryViewModel.From, queryViewModel.To)),
                    HttpContext.RequestAborted)
                .Select(record => (Record: record, Info: dict[record.BrandId]))
                .Select(tup => new AdjustSummaryListItemViewModel
                {
                    BrandId = tup.Record.BrandId,
                    Brand = tup.Info.BrandCode,
                    Licensee = tup.Info.LicenseeCode,
                    Agent = tup.Record.Agent,
                    Company = tup.Record.Company,
                    System = tup.Record.System,
                    Transfer = tup.Record.Transfer,
                    Bonus = tup.Record.Bonus,
                    AgentOther = tup.Record.AgentOther
                }))
            .OrderBy(model => model.Licensee)
            .ThenBy(model => model.Brand);
}
