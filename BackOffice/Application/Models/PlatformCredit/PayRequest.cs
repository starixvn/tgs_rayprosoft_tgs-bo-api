using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public record PayRequest(Guid Id, decimal Amount, string EditorUsername = null) : IRequest<MessageResponse>;
