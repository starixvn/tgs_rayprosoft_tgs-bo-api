using System;

namespace TGS.Services.BackOffice.Domain;
public record TransferBalanceCreateInfo(
    Guid BrandId,
    decimal Amount,
    string DraweeType,
    string Drawee,
    string PayeeType,
    string Payee,
    bool IncludeInNetProfit);
