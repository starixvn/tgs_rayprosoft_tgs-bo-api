using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class GetBrandGamesRequest : PageableRequest, IRequest<PagedResponse<BrandGameModel>>
{
    public Guid? BrandId { get; set; }

    public Guid? GameProviderId { get; set; }

    public GameType? GameType { get; set; }

    public string Keyword { get; set; }
}
