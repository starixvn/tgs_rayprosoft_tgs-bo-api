#nullable enable

namespace TGS.Services.BackOffice.Domain;

public record OfflinePlayerItem(
    string UserName,
    string? PlayerCode);
