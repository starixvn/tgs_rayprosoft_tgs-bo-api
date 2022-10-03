using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

public class UpdateBrandAgentDividendConditionRequest : IRequest<MessageResponse>
{
    public Guid BrandId { get; set; }

    public BrandAgentDividendConditionModel[] BrandAgentDividendConditions { get; init; }
}