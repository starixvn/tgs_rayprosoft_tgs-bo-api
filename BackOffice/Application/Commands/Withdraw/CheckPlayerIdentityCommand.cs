using System;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

public record CheckPlayerIdentityCommand(Guid BrandId, string PlayerCode);
