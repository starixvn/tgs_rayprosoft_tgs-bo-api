using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record GetBrandAllAgentRequest(Guid BrandId) : IRequest<GetBrandAllAgentResponse>;
