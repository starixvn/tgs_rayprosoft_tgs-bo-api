using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

public record GetBrandAgentDividendConditionsRequest(Guid BrandId)
    : IRequest<PagedResponse<BrandAgentDividendConditionModel>>;
