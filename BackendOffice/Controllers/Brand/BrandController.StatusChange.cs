#nullable enable

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using Resource = TGS.Services.BackOffice.Application.Resources;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

public partial class BrandController
{
    [HttpPut("{brandId:guid}/{status}")]
    [Description("change a Brand's status")]
    public async ValueTask<MessageResponse> UpdateBrandStatusAsync(
        [FromServices] IGetService<GetBrandInfoQuery, BrandInfo?> getBrandService,
        [FromServices] ICommandService<UpdateBrandStatusCommand> updateStatusService,
        Guid brandId, 
        BrandStatus status)
    {
        var brandInfo = await getBrandService.GetAsync(new GetBrandInfoQuery(brandId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (brandInfo is null)
            return new MessageResponse
            {
                Valid = false,
                Message = Resource.Brand.NotFound
            };

        if (brandInfo.Status is Tgs.BrandCertificate.BrandStatus.Archived)
            return new MessageResponse
            {
                Valid = false,
                Message = "An archived brand cannot change status"
            };

        var command = new UpdateBrandStatusCommand(
            brandId,
            status,
            Username);

        await updateStatusService.ExecuteAsync(command, HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }
}
