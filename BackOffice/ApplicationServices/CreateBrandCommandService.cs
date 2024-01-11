using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Core.Models.Enums;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using ContractGameType = Tgs.Agent.Dividend.Client.Enums.ContractGameType;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class CreateBrandCommandService(
    IBrandService brandService,
    IBrandContractClient brandContractClient) : IGetService<CreateBrandCommand, Guid>
{
    private readonly IBrandService _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));

    public async ValueTask<Guid> GetAsync(
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

        var id = await _brandService.CreateBrandAsync(createInfo, cancellationToken);

        if (command.AgentDividendType != AgentDividendType.OnePercentage)
        {
            await brandContractClient.CreateBrandContractAsync(new CreateBrandContract(id,
                    "brandContract",
                    (short)command.AgentDividendType,
                    0,
                    0,
                    0,
                    0,
                    command.Modifier,
                    [
                        ContractGameType.Sportsbook,
                        ContractGameType.VirtualSports,
                        ContractGameType.LiveDealer,
                        ContractGameType.CardGame,
                        ContractGameType.Lottery,
                        ContractGameType.Slot
                    ],
                    []),
                cancellationToken);
        }

        return id;
    }
}