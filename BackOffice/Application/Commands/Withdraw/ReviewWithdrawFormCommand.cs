using System;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

public record ReviewWithdrawFormCommand(
    Guid BrandId,
    Guid FormId,
    Guid? BrandPaymentId,
    ReviewState Status,
    string Remark,
    string ReviewUserId,
    string ReviewUserName);
