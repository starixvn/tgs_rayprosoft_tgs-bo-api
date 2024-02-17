using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record UpdateBrandAllAgentSwitchRequest(Guid BrandId, bool Switch) : IRequest<MessageResponse>;
