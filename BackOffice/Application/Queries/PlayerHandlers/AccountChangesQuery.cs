using System;
using Tgs.CashService.Client;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public record AccountChangesQuery(
    string PlayerCode,
    DateTime From,
    DateTime To,
    Domain.Enums.TransactionType? AccountChangeType,
    PagingConditions Paging);
