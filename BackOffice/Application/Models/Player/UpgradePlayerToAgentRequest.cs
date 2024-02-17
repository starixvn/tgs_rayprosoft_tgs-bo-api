#nullable enable

using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Player;

public record UpgradePlayerToAgentRequest(string PlayerCode, string EditorUsername) : IRequest<MessageResponse>;
