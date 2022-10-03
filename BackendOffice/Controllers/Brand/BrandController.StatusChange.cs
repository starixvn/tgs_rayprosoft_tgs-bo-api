using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

public partial class BrandController
{
    [HttpPut("{brandId:guid}/{status}")]
    [Description("change a Brand's status")]
    public Task<MessageResponse> UpdateBrandStatusAsync(Guid brandId, BrandStatus status)
    {
        return _mediator.Send(new UpdateBrandStatusRequest(brandId, status));
    }
}