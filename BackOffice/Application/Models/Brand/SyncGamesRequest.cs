using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public record SyncGamesRequest(Guid BrandId) : IRequest<MessageResponse>;
