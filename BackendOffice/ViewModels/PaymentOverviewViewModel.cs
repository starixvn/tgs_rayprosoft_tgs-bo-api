using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PaymentOverviewViewModel
{
    public decimal? FirstDepositAmount { get; set; }

    public decimal? FirstDepositHandlingFee { get; set; }

    public DateTime? FirstDepositOn { get; set; }

    public decimal? FirstWithdrawAmount { get; set; }

    public decimal? FirstWithdrawHandlingFee { get; set; }

    public DateTime? FirstWithdrawOn { get; set; }

    public DateOnly RecordedBy { get; set; }

    public decimal TotalDepositAmount { get; set; }

    public decimal TotalWithdrawAmount { get; set; }

    public int TotalDepositCount { get; set; }

    public int TotalWithdrawCount { get; set; }
}
