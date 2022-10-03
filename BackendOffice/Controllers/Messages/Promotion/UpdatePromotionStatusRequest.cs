using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public record UpdatePromotionStatusRequest(Guid PromotionId, PromotionStatus Status);