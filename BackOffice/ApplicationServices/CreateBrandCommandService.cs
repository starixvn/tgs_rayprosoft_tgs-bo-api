using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class CreateBrandCommandService : IGetService<CreateBrandCommand, Guid>
{
    private readonly IBrandService _brandService;

    public CreateBrandCommandService(
        IBrandService brandService)
    {
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    }

    public ValueTask<Guid> GetAsync(
        CreateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        var createInfo = new CreateBrandInfo(
            command.Code,
            command.Name,
            command.Description,
            command.OperationalEmail,
            command.PlatformCharge,
            command.TimeSpanMinutes,
            command.EnablePlayerIdentity,
            command.LicenseeId,
            command.CurrencyCodes,
            command.Languages,
            command.SettlementIntervalType,
            command.SettlementDay,
            command.GameProviders,
            command.AgentDividendType,
            command.CheckDividendActiveCount,
            command.AutoClearWageringRequirement,
            command.AutoClearWageringRequirementBalance,
            command.DepositHandlingFeeRateTenPercentile,
            command.WithdrawHandlingFeeRateTenPercentile,
            command.Domains,
            command.BetLimits,
            command.Modifier);

        return _brandService.CreateBrandAsync(createInfo, cancellationToken);
    }
}