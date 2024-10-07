using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandProductFee;

public class GetBrandProductFeeResponse : MessageResponse
{
    public Guid BrandId { get; init; }
    public decimal CardGamePercentage { get; init; }
    public decimal LiveDealerPercentage { get; init; }
    public decimal LotteryPercentage { get; init; }
    public decimal SlotPercentage { get; init; }
    public decimal SportsPercentage { get; init; }
    public decimal VirtualSportsPercentage { get; init; }
}
