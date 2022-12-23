using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class BrandGameProviderReportController : BaseContextController
{
    [HttpPost("generate")]
    [Description("Generate brand game provider round aggregate")]
    public async Task<MessageResponse> GenerateGameProviderAggregateAsync(
        [FromServices] IGetService<GenerateBrandGameProviderAggregateRequest, MessageResponse>
            generateBrandGameProviderAggregateService,
        [FromBody] GenerateBrandGameProviderAggregateRequest request)
    {
        request.UserId = UserId;
        return await generateBrandGameProviderAggregateService
            .GetAsync(request)
            .ConfigureAwait(false);
    }

    [HttpGet]
    [Description("Get brand game provider round aggregate")]
    public async Task<GetBrandGameProviderRoundsReportResponse> GetGameProviderRoundsAsync(
        [FromServices]
        IGetService<GetBrandGameProviderRoundsReportRequest, GetBrandGameProviderRoundsReportResponse>
            brandGameProviderRoundsService,
        [FromQuery] GetBrandGameProviderRoundsReportRequest request)
        => await brandGameProviderRoundsService
            .GetAsync(request)
            .ConfigureAwait(false);
}
