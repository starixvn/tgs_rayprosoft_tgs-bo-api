using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

public class UpdateBrandAgentDividendConditionRequest : IRequest<MessageResponse>
{
    public BrandAgentDividendConditionModel[] BrandAgentDividendConditions { get; init; }

    public Guid BrandId { get; set; }
}
