
using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandProductFee;

public record UpdateBrandProductFeeRequest
{
	public Guid BrandId { get; init; }
	public decimal CardGamePercentage { get; init; }
	public decimal LiveDealerPercentage { get; init; }
	public decimal LotteryPercentage { get; init; }
	public decimal SlotPercentage { get; init; }
	public decimal SportsPercentage { get; init; }
	public decimal VirtualSportsPercentage { get; init; }
}
