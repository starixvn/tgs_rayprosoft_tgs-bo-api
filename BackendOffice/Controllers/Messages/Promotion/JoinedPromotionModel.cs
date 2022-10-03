using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class JoinedPromotionModel
{
    public string PromotionName { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}