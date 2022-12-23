using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class LicenseeRequest
{
    public int[] BetLimitIds { get; set; }

    public string Code { get; set; }

    public string[] CultureCodes { get; set; }

    public string[] CurrencyCodes { get; set; }

    public string Description { get; set; }

    public Guid[] GameProviderIds { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string OperationalEmail { get; set; }

    public Guid ResellerId { get; set; }
}
