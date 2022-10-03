using System;
using MediatR;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record GetBrandAllAgentWinLossSettingRequest
    (Guid BrandId, AllAgentGameType AllAgentGameType) : IRequest<WinLossConditionModel[]>;