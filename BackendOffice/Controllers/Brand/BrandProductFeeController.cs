using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandProductFee;

namespace TGS.Apis.BackendOffice.Controllers.Brand
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandProductFeeController(IBrandServiceClient client) : BaseContextController
    {
        [HttpGet("{brandId}", Name = "GetBrandProductFee")]
        public async Task<GetBrandProductFeeResponse> GetBrandProductFeeAsync(Guid brandId)
        {
            var response = await client.GetBrandProductFeeInfoAsync(brandId, HttpContext.RequestAborted);

            return response == null
                ? new GetBrandProductFeeResponse{ BrandId = brandId }
                : new GetBrandProductFeeResponse
                {
                    BrandId = brandId,
                    CardGamePercentage = response.CardGamePercentage,
                    LiveDealerPercentage = response.LiveDealerPercentage,
                    LotteryPercentage = response.LotteryPercentage,
                    SlotPercentage = response.SlotPercentage,
                    SportsPercentage = response.SportsPercentage,
                    VirtualSportsPercentage = response.VirtualSportsPercentage,
                };
        }

        [HttpPut(Name = "UpdateBrandProductFee")]
        public ValueTask UpdateBrandProductFeeAsync(
            UpdateBrandProductFeeRequest request)
        {
            return client.CreateUpdateBrandProductFeeAsync(new CreateUpdateBrandProductFeeRequest
            {
                BrandId = request.BrandId,
                CardGamePercentage = request.CardGamePercentage,
                LiveDealerPercentage = request.LiveDealerPercentage,
                LotteryPercentage = request.LotteryPercentage,
                SlotPercentage = request.SlotPercentage,
                SportsPercentage = request.SportsPercentage,
                VirtualSportsPercentage = request.VirtualSportsPercentage,
                EditorUsername = Username
            });
        }
    }
}
