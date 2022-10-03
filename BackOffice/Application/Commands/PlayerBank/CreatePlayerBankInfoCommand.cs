using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

public record CreatePlayerBankInfoCommand(
    string PlayerCode,
    Guid BrandId,
    Guid PaymentChannelId,
    string BankType,
    JsonDocument Detail,
    string ModifyUser);
