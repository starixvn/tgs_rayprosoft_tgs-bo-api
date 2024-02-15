using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record UpdateBrandAllAgentGameTypeRequest(
    Guid BrandId,
    bool Switch,
    bool HasValidBet,
    bool HasWinLoss,
    AllAgentGameType AllAgentGameType)
    : IRequest<MessageResponse>;
