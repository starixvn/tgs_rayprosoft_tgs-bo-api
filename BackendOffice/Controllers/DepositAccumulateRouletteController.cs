using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.DepositAccumulateRoulette;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/dar")]
[ApiController]
public class DepositAccumulateRouletteController : BaseContextController
{
    [HttpGet("detail/{brandId}/{recordId}", Name = "GetDepositAccumulateRouletteGameRecordDetail")]
    public async ValueTask<GenericResponse<DepositAccumulateRouletteGameRecordDetailViewModel>> GetDepositAccumulateRouletteGameRecordDetailAsync(
        [FromServices] IGetService<DepositAccumulateRouletteRecordDetailQuery, DepositAccumulateRouletteGameRecordDetailInfo> getService,
        Guid brandId,
        Guid recordId)
    {
        var detailInfo = await getService.GetAsync(
                new DepositAccumulateRouletteRecordDetailQuery(brandId, recordId),
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new GenericResponse<DepositAccumulateRouletteGameRecordDetailViewModel>
        {
            Valid = true,
            Data = new DepositAccumulateRouletteGameRecordDetailViewModel
            {
                WinningPrize = detailInfo.WinningPrize
            }
        };
    }
}
