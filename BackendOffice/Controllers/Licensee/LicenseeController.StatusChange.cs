using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Licensee;

public partial class LicenseeController
{
    [HttpPut("{licenseeId:guid}/{status}")]
    [Description("change a Licensee's status")]
    public async Task<MessageResponse> UpdateLicenseeStatusAsync(Guid licenseeId, Status status)
    {
        var licensee = await _service.GetLicenseeAsync(q => q.Id == licenseeId);

        if (licensee == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Licensee.NotFound);
        }

        await _service.ChangeStatusAsync(licensee, status, Username);

        return CreateValidResponse<MessageResponse>();
    }
}