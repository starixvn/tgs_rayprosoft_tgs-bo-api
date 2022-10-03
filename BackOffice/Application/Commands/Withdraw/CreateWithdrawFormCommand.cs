using System;
using System.Threading.Tasks;

#nullable enable

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

public record CreateWithdrawFormCommand(
    string PlayerCode,
    Guid BrandId,
    Guid BankInfoId,
    decimal Amount,
    string? Note)
{
    public TaskCompletionSource<Guid> ReturnFormIdSource { get; } = new TaskCompletionSource<Guid>();
}
