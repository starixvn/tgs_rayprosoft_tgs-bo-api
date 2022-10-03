using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PromotionDispatch;

public class DispatchPromotionBonusRequest
{
    public Guid OperatorUserId { get; set; }
    public DispatchPlayer[] Players { get; set; }
}

public class DispatchPlayer
{
    public Guid PromotionBonusId { get; set; }
    public string PlayerCode { get; set; }
}