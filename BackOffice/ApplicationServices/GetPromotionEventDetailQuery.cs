using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetPromotionEventDetailQuery(
    Guid PromotionId, 
    Guid EventId);