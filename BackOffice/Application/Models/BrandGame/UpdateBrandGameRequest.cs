using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class UpdateBrandGameRequest : IRequest<MessageResponse>
{
    public Guid Id { get; set; }
    public decimal EffectiveBettingPercentage { get; set; }
    public bool Rebate { get; set; }
}