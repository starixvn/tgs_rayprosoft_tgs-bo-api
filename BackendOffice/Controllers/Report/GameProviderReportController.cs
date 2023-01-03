using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Aggregate;
using TGS.Services.BackOffice.Application.Models.ReportGameProviderRound;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class GameProviderReportController : BaseContextController
{
    [HttpPost("generate")]
    [Description("Generate game provider round aggregate")]
    public async Task<MessageResponse> GenerateGameProviderAggregateAsync(
        [FromServices] IGetService<GenerateGameProviderAggregateRequest, MessageResponse>
            generateGameProviderAggregateService,
        [FromBody] GenerateGameProviderAggregateRequest request)
    {
        request.UserId = UserId;
        return await generateGameProviderAggregateService
            .GetAsync(request)
            .ConfigureAwait(false);
    }

    [HttpGet]
    [Description("Get game provider round aggregate")]
    public async Task<PagedResponse<GameProviderRoundsReportModel>> GetGameProviderRoundsAsync(
        [FromServices] IGetService<GetGameProviderRoundsReportRequest, PagedResponse<GameProviderRoundsReportModel>>
            gameProviderRoundsService,
        [FromQuery] GetGameProviderRoundsReportRequest request)
        => await gameProviderRoundsService
            .GetAsync(request)
            .ConfigureAwait(false);
}
