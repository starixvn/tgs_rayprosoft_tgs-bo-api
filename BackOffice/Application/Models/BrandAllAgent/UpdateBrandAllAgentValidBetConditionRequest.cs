using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record UpdateBrandAllAgentValidBetConditionRequest(
    Guid BrandId,
    ValidBetConditionModel[] Conditions,
    AllAgentGameType AllAgentGameType)
    : IRequest<MessageResponse>;
