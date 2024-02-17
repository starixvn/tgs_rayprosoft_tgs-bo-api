using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetBrandPaymentsRequest : PageableRequest
{
    public Guid? BrandId { get; set; }

    public string Keyword { get; set; }

    public SwitchStatus? PaymentStatus { get; set; }

    public PaymentTransferType? TransferType { get; set; }
}
