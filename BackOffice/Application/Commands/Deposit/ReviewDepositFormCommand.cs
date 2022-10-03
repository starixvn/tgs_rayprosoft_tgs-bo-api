using System;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Application.Commands.Deposit;

public record ReviewDepositFormCommand(
    Guid BrandId,
    Guid FormId,
    ReviewState Status,
    decimal? ActualAmount,
    string Remark,
    string ReviewUserId,
    string ReviewUserName);