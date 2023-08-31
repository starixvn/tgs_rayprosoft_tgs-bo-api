using System.Collections.Generic;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

public record ExportDepositFormsCommand(IEnumerable<DepositFormInfo> DepositForms, decimal TotalAmount);
