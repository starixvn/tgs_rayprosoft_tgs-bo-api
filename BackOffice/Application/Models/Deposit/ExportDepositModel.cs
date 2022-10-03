using System;

namespace TGS.Services.BackOffice.Application.Models.Deposit;

public class ExportDepositModel
{
    public decimal? ActualAmount { get; init; }

    public decimal Amount { get; init; }

    public DateTime ApplicationTime { get; init; }

    public decimal? HandlingFee { get; init; }

    public string Id { get; init; }

    public string OrderNo { get; init; }

    public string PaymentMethod { get; init; }

    public string PaymentName { get; init; }

    public string Remark { get; init; }

    public string ReviewedUserName { get; init; }

    public int SerialNumber { get; init; }

    public string Status { get; init; }

    public string ThirdPartyOrderNo { get; init; }

    public DateTime UpdateTime { get; init; }

    public string UserName { get; init; }
}
