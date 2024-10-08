using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Filters;
using TGS.Services.BackOffice.Application.Models.PlayerOnline;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerOnlineController : ControllerBase
{
    [BOTypeFilter(typeof(MerchanPermissionQueryFiler), Arguments = ["brand", "brandId"])]
    [HttpGet]
    [Description("Get Player Online ")]
    public ValueTask<PlayerOnlineModel> GetPlayerIpsListAsync(
        [FromServices] IGetService<GetPlayerOnlineRequest, PlayerOnlineModel> getService,
        [FromQuery] GetPlayerOnlineRequest request)
        => getService.GetAsync(request);
}