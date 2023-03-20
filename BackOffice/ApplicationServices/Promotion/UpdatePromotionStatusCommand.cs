using System;
using Tgs.Promotion.Clients;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record UpdatePromotionStatusCommand(
    Guid PromotionId,
    PromotionStatus Status,
    string Modifier);