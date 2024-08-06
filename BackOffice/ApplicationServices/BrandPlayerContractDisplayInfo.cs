using Tgs.PlayerService.Client;

namespace TGS.Services.BackOffice.ApplicationServices;

public record BrandPlayerContractDisplayInfo(
    PlayerPropertyName Name,
    bool IsVisualized);