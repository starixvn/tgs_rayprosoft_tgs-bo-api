using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Logging;
using TGS.Apis.BackendOffice.Utils;
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
        LogAdmin(paymentId, status);
        return CreateValidResponse<MessageResponse>();
    }

    private void LogAdmin(Guid brandPaymentId, SwitchStatus status) {

        var ip = IPUtils.GetRequestIP(HttpContext) ?? string.Empty;
        var accountName = HttpContext.User.GetUserName();
        var description = $"User: {accountName} | Action: Switch brand payment status to {status}";

        var adminLogEvent = new AdminLogEvent() {
            TrackingId = brandPaymentId,
            ServerName = Environment.MachineName,
            IpAddress = ip is null ? "" : ip.ToString(),
            Description = description,
            Method = HttpContext.Request.Method,
            Path = HttpContext.Request.Path,
            RecordedOn = DateTimeOffset.Now,
            ResponseCode = (short)HttpContext.Response.StatusCode
        };
        tgsLog.LogAdmin(adminLogEvent);
    }
}
