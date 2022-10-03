using System;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class BrandGroupGameProviderBetLimitModel
{
    public int BrandBetLimitId { get; set; }
    public Guid GameProviderBetLimitId { get; set; }
    public Guid GameProviderId { get; set; }
}