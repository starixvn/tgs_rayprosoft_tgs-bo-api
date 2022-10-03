using System;

namespace TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

public record CreateIdentityAuthenticationFormCommand(
    string PlayerCode,
    Guid BrandId,
    string FileId);
