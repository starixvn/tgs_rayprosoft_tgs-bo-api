using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Reseller;

public partial class ResellerController
{
    [HttpPut("{resellerId:guid}/{status}")]
    [Description("change a Reseller's status")]
    public async Task<MessageResponse> UpdateResellerStatusAsync(Guid resellerId, Status status)
    {
        var reseller = await service.GetResellerAsync(q => q.Id == resellerId);

        if (reseller == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Reseller.NotFound);
        }

        await service.ChangeStatusAsync(reseller, status, Username);

        return CreateValidResponse<MessageResponse>();
    }
}
