using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TGS.Apis.BackendOffice.Controllers;
[Route("api/offline-deposit-reward")]
[ApiController]
public class OfflineDepositRewardController : ControllerBase
{
    [HttpPost]
    public void UploadPlayerList(
        Guid brandId,
        [FromForm] IFormFile playerListExcel)
    {

    }
}
