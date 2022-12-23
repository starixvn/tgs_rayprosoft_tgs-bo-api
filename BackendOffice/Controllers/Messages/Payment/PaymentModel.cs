using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Payment;

public class PaymentModel
{
    public string Code { get; set; }

    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string Description { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
