using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

public class UpdateBrandAgentDividendConditionRequest : IRequest<MessageResponse>
{
    public BrandAgentDividendConditionModel[] BrandAgentDividendConditions { get; init; }

    public Guid BrandId { get; set; }
}
