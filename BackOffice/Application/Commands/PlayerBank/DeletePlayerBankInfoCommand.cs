using System;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

public record DeletePlayerBankInfoCommand(string PlayerCode, Guid BankInfoId);
