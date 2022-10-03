using System;

namespace TGS.Services.BackOffice.Application.Models.Withdraw;

public class ExportWithdrawModel
{
    public decimal Amount { get; init; }

    public DateTime ApplicationTime { get; init; }

    public string Id { get; init; }

    public string OrderNo { get; init; }

    public string Remark { get; init; }

    public int SerialNumber { get; init; }

    public string Status { get; init; }

    public string ThirdPartyOrderNo { get; init; }

    public DateTime UpdateTime { get; init; }

    public string Username { get; init; }

    public string UserType { get; init; }
}
