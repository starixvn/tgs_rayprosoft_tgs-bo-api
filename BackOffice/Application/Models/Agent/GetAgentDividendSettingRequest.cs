using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public record GetAgentDividendSettingRequest
    (Guid BrandId, string Username) : IRequest<GetAgentDividendSettingResponse>;
