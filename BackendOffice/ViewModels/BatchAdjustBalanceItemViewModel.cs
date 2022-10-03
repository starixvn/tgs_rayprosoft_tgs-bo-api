namespace TGS.Apis.BackendOffice.ViewModels;

#nullable enable

public class BatchAdjustBalanceItemViewModel
{
    public decimal AdjustAmount { get; init; }

    public int Id { get; init; }

    public string? Remark { get; init; }

    public decimal Turnover { get; init; }

    public string UserName { get; init; } = default!;
}
