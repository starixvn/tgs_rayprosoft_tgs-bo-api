using System;
using System.Collections.Generic;
using Tgs.CashService.Client;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public record AccountChangesQuery(
    Guid WalletId,
    DateTime From,
    DateTime To,
    IEnumerable<int> QueryTransactionTypes,
    PagingConditions Paging);
