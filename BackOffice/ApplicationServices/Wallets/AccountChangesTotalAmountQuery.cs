using System;

namespace TGS.Services.BackOffice.ApplicationServices.Wallets;

public record AccountChangesTotalAmountQuery(
    string PlayerCode,
    DateTime From,
    DateTime To,
    Domain.Enums.TransactionType? AccountChangeType);
