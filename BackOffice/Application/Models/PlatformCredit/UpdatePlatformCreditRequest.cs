using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public record UpdatePlatformCreditRequest(
    Guid Id,
    decimal Credit,
    string EditorUsername = null)
    : IRequest<MessageResponse>;
