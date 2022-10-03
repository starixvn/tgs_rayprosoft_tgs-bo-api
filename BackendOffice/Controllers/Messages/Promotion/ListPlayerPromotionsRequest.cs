using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion
{
    public class ListPlayerPromotionsRequest
    {
        public Guid BrandId { get; set; }

        public string PlayerCode { get; set; }
    }
}
