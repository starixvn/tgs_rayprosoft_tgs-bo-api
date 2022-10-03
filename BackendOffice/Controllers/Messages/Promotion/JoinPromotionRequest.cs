using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public record JoinPromotionRequest(Guid PromotionId, string PlayerCode);