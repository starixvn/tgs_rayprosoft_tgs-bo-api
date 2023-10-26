#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerAgentCommand(
    string PlayerCode,
    string? AgentUsername,
    string ModifyUserId,
    string ModifyUsername);
