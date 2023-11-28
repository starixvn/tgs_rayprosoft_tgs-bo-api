using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PromotionDispatch;

public class DispatchPromotionBonusViewRequest
{
    public Guid OperatorUserId { get; set; }

    public DispatchPlayerView[] Players { get; set; }
}

public class DispatchPlayerView
{
    public string PlayerCode { get; set; }

    public Guid PromotionBonusId { get; set; }
}
