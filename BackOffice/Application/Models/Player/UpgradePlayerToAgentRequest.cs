#nullable enable

using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Player;

public record UpgradePlayerToAgentRequest(string PlayerCode, string EditorUsername) : IRequest<MessageResponse>;
