using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class ResellerRequest
{
    public int[] BetLimitIds { get; set; }

    public BusinessType BusinessType { get; set; }

    public string Code { get; set; }

    public string[] CultureCodes { get; set; }

    public string[] CurrencyCodes { get; set; }

    public string Description { get; set; }

    public Guid[] GameProviderIds { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string OperationalEmail { get; set; }
}
