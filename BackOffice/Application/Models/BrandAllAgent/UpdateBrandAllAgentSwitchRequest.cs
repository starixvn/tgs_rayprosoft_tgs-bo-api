using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record UpdateBrandAllAgentSwitchRequest(Guid BrandId, bool Switch) : IRequest<MessageResponse>;