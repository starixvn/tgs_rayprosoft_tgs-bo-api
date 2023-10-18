# nullable enable

using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record SetPlayerContactCommand(
    string PlayerCode,
    string ModifyUserId,
    string ModifyUsername,
    IDictionary<string, string?> ModifyContact);