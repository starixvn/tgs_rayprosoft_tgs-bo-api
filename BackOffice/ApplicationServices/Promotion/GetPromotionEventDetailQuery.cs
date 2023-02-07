using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

public record GetPromotionEventDetailQuery(
    Guid PromotionId,
    Guid EventId);
