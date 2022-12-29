using System;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class BrandGroupModel
{
    public BrandGameModel[] BrandGames { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    public BrandGroupGameProviderBetLimitModel[] GameProviderBetLimits { get; set; }

    public Guid Id { get; set; }

    public bool IsSports { get; set; }

    public string Name { get; set; }
}
