using System;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class CurrencyModel
{
    public string Code { get; set; }

    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string Name { get; set; }

    public string NativeName { get; set; }

    public string Symbol { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
