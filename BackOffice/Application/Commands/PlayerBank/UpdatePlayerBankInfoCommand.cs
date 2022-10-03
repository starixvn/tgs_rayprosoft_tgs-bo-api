using System;
using System.Text.Json;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

public record UpdatePlayerBankInfoCommand(
    string PlayerCode,
    Guid BankInfoId,
    Guid BrandId,
    Guid PaymentChannelId,
    string BankType,
    JsonDocument Detail,
    string ModifyUser);
