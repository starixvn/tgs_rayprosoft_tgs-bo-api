using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Client;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportAgentWinLoss;

public class ExportAgentWinLossQuery : IGetService<ExportAgentWinLossRequest, Stream>
{
    private readonly IAgentService _agentService;
    private readonly IBrandService _brandService;
    private readonly IPlayerService _playerService;
    private readonly IReportClient _reportClient;

    public ExportAgentWinLossQuery(
        IReportClient reportClient,
        IBrandService brandService,
        IAgentService agentService,
        IPlayerService playerService)
    {
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
    }

    public async ValueTask<Stream> GetAsync(ExportAgentWinLossRequest request,
        CancellationToken cancellationToken = default)
    {
        var platformCharge = await _brandService.GetBrandQueries().Where(x => x.Id == request.BrandId)
            .Select(x => x.PlatformCharge).FirstOrDefaultAsync(cancellationToken);

        switch (string.IsNullOrEmpty(request.Username))
        {
            case true:
                return await _reportClient.ExportMasterAgentWinLossAsync(new(
                    request.BrandId,
                    platformCharge,
                    request.From,
                    request.To
                ));

            case false when !request.DownLine && request.MemberType == AccountType.Agent:
                var agent = await _agentService.GetAgentAsync(x
                    => x.BrandId == request.BrandId && x.Username == request.Username);

                return await _reportClient.ExportAgentWinLossAsync(new(
                    request.BrandId,
                    agent.Id,
                    platformCharge,
                    request.From,
                    request.To
                ));

            case false when request.DownLine && request.MemberType == AccountType.Agent:
                var downLineAgent = await _agentService.GetAgentAsync(x
                    => x.BrandId == request.BrandId && x.Username == request.Username);

                return await _reportClient.ExportDownLineAgentWinLossAsync(new(
                    request.BrandId,
                    downLineAgent.Id,
                    platformCharge,
                    request.From,
                    request.To
                ));

            case false when request.MemberType == AccountType.Player:
                var playerCode = await _playerService.GetPlayerQueries()
                    .Where(x => x.BrandId == request.BrandId && x.Username == request.Username)
                    .Select(x => x.PlayerCode)
                    .FirstOrDefaultAsync(cancellationToken);

                return await _reportClient.ExportPlayerWinLossAsync(new(
                    playerCode,
                    platformCharge,
                    request.From,
                    request.To
                ));
        }

        return default;
    }
}
