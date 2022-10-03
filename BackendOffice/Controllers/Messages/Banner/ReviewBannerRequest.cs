using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Banner;

public class ReviewBannerRequest
{
    public string Remark { get; set; }

    public BannerReviewState ReviewState { get; set; }
}
