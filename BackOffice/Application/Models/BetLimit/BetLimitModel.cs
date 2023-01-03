using System;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class BetLimitModel
{
    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string CurrencyCode { get; set; }

    public string CurrencyName { get; set; }

    public int Id { get; set; }

    public int MaxBet { get; set; }

    public int MaxBetPerMatch { get; set; }

    public int MinBet { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
