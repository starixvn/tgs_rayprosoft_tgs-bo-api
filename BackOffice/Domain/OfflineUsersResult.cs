using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;
public record OfflineUsersResult(
    IEnumerable<OfflinePlayerItem> Players,
    int Matchs,
    IEnumerable<string> Duplicates);
