# nullable enable
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerInfoCommand(
    string PlayerCode,
    string ModifyUserId,
    string ModifyUsername,
    IDictionary<string, string?> ModifyInfo);
