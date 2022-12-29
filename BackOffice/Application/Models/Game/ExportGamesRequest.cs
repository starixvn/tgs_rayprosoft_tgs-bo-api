using System;
using System.IO;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Game;

public record ExportGamesRequest(Guid Id) : IRequest<MemoryStream>;
