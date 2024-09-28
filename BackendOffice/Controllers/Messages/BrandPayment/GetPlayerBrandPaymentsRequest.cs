using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetPlayerBrandPaymentsRequest
{
    public Guid BrandId { get; set; }

    public string PlayerCode { get; set; }

    public PaymentTransferType TransferType { get; set; } = PaymentTransferType.Deposit;
}
