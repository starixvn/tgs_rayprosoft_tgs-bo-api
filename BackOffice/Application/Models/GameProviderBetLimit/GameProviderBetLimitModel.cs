using System;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GameProviderBetLimitModel
{
    public Guid? Id { get; set; }
    public string CurrencyCode { get; set; }
    public int MinBet { get; set; }
    public int MaxBet { get; set; }
    public int? MaxBetPerMatch { get; set; }
    public string ExternalSetting { get; set; }
}