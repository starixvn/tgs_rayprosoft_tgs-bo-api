using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class CreateBrandCommandService(
    IBrandService brandService,IBrandServiceClient brandServiceClient) : IGetService<CreateBrandCommand, Guid>
{
    private readonly IBrandService _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));

    public async ValueTask<Guid> GetAsync(
        CreateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        var createBrand = new CreateBrandFullInfoRequest
        {
            Code = command.Code, Name = command.Name,
            Description = command.Description,
            OperationalEmail = command.OperationalEmail,
            TimeSpanMinutes = command.TimeSpanMinutes,
            EnablePlayerIdentity = command.EnablePlayerIdentity,
            LicenseeId = command.LicenseeId,
            SettlementIntervalType = (SettlementIntervalType)command.SettlementIntervalType,
            SettlementDay = command.SettlementDay,
            AgentDividendType = (AgentDividendType)command.AgentDividendType,
            CheckDividendActiveCount = command.CheckDividendActiveCount,
            AutoClearWageringRequirement = command.AutoClearWageringRequirement,
            AutoSendAgentBonus = command.AutoSendAgentBonus,
            EnablePlayerAgent = command.EnablePlayerAgent,
            AutoClearWageringRequirementBalance = command.AutoClearWageringRequirementBalance,
            DepositHandlingFeeRateTenPercentile = command.DepositHandlingFeeRateTenPercentile,
            WithdrawHandlingFeeRateTenPercentile = command.WithdrawHandlingFeeRateTenPercentile,
            EditorUsername = command.Modifier
        };

        var brandId = await brandServiceClient.CreateBrandAsync(createBrand, cancellationToken);

        var createInfo = new CreateBrandInfo(
            brandId,
            command.CurrencyCodes,
            command.Languages,
            command.GameProviders,
            command.Domains,
            command.BetLimits,
            command.Modifier);

        var id = await _brandService.CreateBrandAsync(createInfo, cancellationToken);

        return id;
    }
}