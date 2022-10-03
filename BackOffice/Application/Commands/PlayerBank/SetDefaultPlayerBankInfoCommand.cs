using System;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

public record SetDefaultPlayerBankInfoCommand(
    string PlayerCode,
    Guid BankInfoId,
    string ModifyUser);
