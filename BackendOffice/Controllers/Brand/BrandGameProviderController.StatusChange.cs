using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

public partial class BrandGameProviderController
{
    [HttpPut("{id}/{status}")]
    [Description("Change a brand game provider status")]
    public Task<MessageResponse> UpdateBrandGameProviderStatusAsync(Guid id, Status status)
        => mediator.Send(new UpdateBrandGameProviderStatusRequest(id, status));
}
