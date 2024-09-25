using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Apis.BackendOffice.Controllers.BrandGameCredits;

[Route("api/[controller]")]
[ApiController]
public class BrandGameCreditTxnController : BaseContextController
{
    [HttpGet("{brandId:guid}/query", Name = "BrandGameCreditTxn_Query")]
    public async ValueTask<PagedResponse<BrandGameCreditTxnViewModel>> QueryAsync(
        [FromServices] IQueryService<BrandGameCreditTxnQuery, BrandGameCreditTxnResult> queryService,
        [FromServices] IGetService<BrandGameCreditTxnQuery, int> countService,
        Guid brandId,
        [FromQuery] BrandGameCreditTxnQueryViewModely source
        )
    {
        var query = new BrandGameCreditTxnQuery(
            BrandId: brandId,
            StartOn: source.StartOn,
            EndOn: source.EndOn,
            Type: source.Type,
            PageNumber: source.PageNumber,
            PageSize: source.PageSize);

        var results = await queryService.QueryAsync(
            query: query,
            cancellationToken: HttpContext.RequestAborted)
            .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BrandGameCreditTxnResult>();

        if (!results.Any())
        {
            return new PagedResponse<BrandGameCreditTxnViewModel>
            {
                Valid = true,
                TotalCount = 0,
                Items = Enumerable.Empty<BrandGameCreditTxnViewModel>().ToArray()
            };
        }

        var count = await countService.GetAsync(
            query: query,
            cancellationToken: HttpContext.RequestAborted)
        .ConfigureAwait(false);

        return new PagedResponse<BrandGameCreditTxnViewModel>
        {
            Valid = true,
            TotalCount = count,
            Items = results.Select(
                selector => new BrandGameCreditTxnViewModel(selector.Id,
                selector.ResellerId,
                selector.ResellerCode,
                selector.LicenseeId,
                selector.LicenseeCode,
                selector.BrandId,
                selector.BrandCode,
                selector.Type,
                selector.Money,
                selector.Amount,
                selector.Remark,
                selector.Expansions,
                selector.CreatedUserId,
                selector.CreatedUsername,
                selector.CreatedOn))
            .ToArray()
        };
    }

    [HttpPost("{brandId:guid}/credit")]
    public ValueTask AddCreditAsync(
        [FromServices] ICommandService<BrandGameCreditTxnAddCreditCommand> service,
        Guid brandId,
        [FromBody] BrandGameCreditTxnAddViewModel source)
    {
        var command = new BrandGameCreditTxnAddCreditCommand(
            BrandId: brandId,
            Money: source.Money,
            Amount: source.Amount,
            Remark: source.Remark,
            Expansions: source.Expansions,
            CreatedUserId: Guid.Parse(UserId),
            CreatedUsername: Username);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: HttpContext.RequestAborted);
    }

    [HttpPost("{brandId:guid}/debit")]
    public ValueTask AddCreditAsync(
        [FromServices] ICommandService<BrandGameCreditTxnAddDebitCommand> service,
        Guid brandId,
        [FromBody] BrandGameCreditTxnAddViewModel source)
    {
        var command = new BrandGameCreditTxnAddDebitCommand(
            BrandId: brandId,
            Money: source.Money,
            Amount: source.Amount,
            Remark: source.Remark,
            Expansions: source.Expansions,
            CreatedUserId: Guid.Parse(UserId),
            CreatedUsername: Username);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: HttpContext.RequestAborted);
    }
}
