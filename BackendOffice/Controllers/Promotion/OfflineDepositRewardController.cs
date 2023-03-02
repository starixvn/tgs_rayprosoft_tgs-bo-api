using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Controllers;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;
[Route("api/offline-deposit-reward")]
[ApiController]
public class OfflineDepositRewardController : ControllerBase
{
    [HttpPost("{brandId}", Name = "UploadOfflineDepositRewardPlayerList")]
    public void UploadPlayerList(
        Guid brandId,
        [FromForm] IFormFile playerListExcel)
    {

    }
}
