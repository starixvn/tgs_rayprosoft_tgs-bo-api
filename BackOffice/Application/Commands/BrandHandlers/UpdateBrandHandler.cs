using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandHandlers;

public class UpdateBrandHandler(
    IBrandService service,
    IBrandServiceClient brandServiceClient,
    IHttpContextAccessor accessor)
    : Handler<UpdateBrandRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await service.GetBrandAsync(q => q.Id == request.Id);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        if (await service.ExistsAsync(x => request.Id != x.Id && x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NameExisted);
        }

        if (await service.ExistsAsync(x => request.Id != x.Id && x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.CodeExisted);
        }

        var updateBrand = new UpdateBrandFullInfoRequest
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            OperationalEmail = request.OperationalEmail,
            PlatformCharge = request.PlatformCharge,
            EnablePlayerIdentity = request.EnablePlayerIdentity,
            SettlementIntervalType = (SettlementIntervalType)request.SettlementIntervalType,
            SettlementDay = request.SettlementDay,
            CheckDividendActiveCount = request.CheckDividendActiveCount,
            AutoClearWageringRequirement = request.AutoClearWageringRequirement,
            AutoSendAgentBonus = request.AutoSendAgentBonus,
            EnablePlayerAgent = request.EnablePlayerAgent,
            AutoClearWageringRequirementBalance = request.AutoClearWageringRequirementBalance,
            DepositHandlingFeeRateTenPercentile = request.DepositHandlingFeeRateTenPercentile,
            WithdrawHandlingFeeRateTenPercentile = request.WithdrawHandlingFeeRateTenPercentile,
            EditorUsername = request.EditorUsername
        };

        var brandId = await brandServiceClient.UpdateBrandAsync(updateBrand, cancellationToken);

        await service.UpdateBrandAsync(brandId, request);

        return CreateValidResponse<MessageResponse>();
    }
}
