using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportGameAction;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class GameActionReportController : BaseContextController
{
    [HttpGet]
    [Description("Get game actions")]
    public async Task<PagedResponse<GameActionReportModel>> GetGameActionsAsync(
        [FromServices] IGetService<GetGameActionsReportRequest, PagedResponse<GameActionReportModel>>
            gameActionsService,
        [FromQuery] GetGameActionsReportRequest request)
        => await gameActionsService
            .GetAsync(request)
            .ConfigureAwait(false);
}
