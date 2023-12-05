using System;
using Tgs.CashService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.Wallets;

public record AccountChangesQuery(
    string PlayerCode,
    DateTime From,
    DateTime To,
    Domain.Enums.TransactionType? AccountChangeType,
    PagingConditions Paging);
