using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

public partial class BrandPaymentController
{
    [HttpPut("{paymentId:guid}/{status}")]
    [Description("change a Payment's status")]
    public async Task<MessageResponse> UpdateBrandPaymentStatusAsync(Guid paymentId, SwitchStatus status)
    {
        var payment = await brandPaymentService.GetPaymentAsync(q => q.Id == paymentId);

        if (payment == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.BrandPayment.PaymentNotFound);
        }

        await brandPaymentService.ChangeStatusAsync(payment, status, Username);

        return CreateValidResponse<MessageResponse>();
    }
}
