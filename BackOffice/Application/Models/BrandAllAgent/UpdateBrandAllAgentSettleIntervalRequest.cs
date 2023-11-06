using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record UpdateBrandAllAgentSettleIntervalRequest(Guid BrandId, SettlementIntervalType SettlementIntervalType) : IRequest<MessageResponse>;
