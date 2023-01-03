using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

public record GetBrandAgentDividendConditionsRequest
    (Guid BrandId) : IRequest<PagedResponse<BrandAgentDividendConditionModel>>;
