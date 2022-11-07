using System.Collections.Generic;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

public record ExportWithdrawFormsCommand(
    IEnumerable<WithdrawFormInfo> WithdrawForms,
    decimal TotalAmount);
