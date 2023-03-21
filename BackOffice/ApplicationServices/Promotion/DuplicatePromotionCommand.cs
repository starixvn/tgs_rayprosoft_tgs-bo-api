using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record DuplicatePromotionCommand(
    Guid BasePromotionId,
    string Modifier);