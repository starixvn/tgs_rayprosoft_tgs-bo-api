namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;

public record CreateBrandContractConditionModel(
    int Rank,
    int ActiveNumber,
    decimal DividendPercentage,
    decimal NetProfit,
    decimal WinLossAmount
);