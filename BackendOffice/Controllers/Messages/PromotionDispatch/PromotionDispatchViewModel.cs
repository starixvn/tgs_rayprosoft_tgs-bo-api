using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PromotionDispatch;

public class PromotionDispatchViewModel
{
    public decimal BonusAmount { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string PlayerCode { get; set; }

    public Guid PromotionBonusId { get; set; }

    public string PromotionGroup { get; set; }

    public Guid PromotionId { get; set; }

    public string PromotionName { get; set; }

    public string PromotionType { get; set; }

    public DateTimeOffset? SentOn { get; set; }

    public string Username { get; set; }
}
