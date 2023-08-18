using System;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public record AccountChangesTotalAmountQuery(
    string PlayerCode,
    DateTime From,
    DateTime To,
    Domain.Enums.TransactionType? AccountChangeType);
