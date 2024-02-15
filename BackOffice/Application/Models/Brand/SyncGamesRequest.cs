using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public record SyncGamesRequest(Guid BrandId) : IRequest<MessageResponse>;
