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

public class ExportAgentWinLossQuery(
    IReportClient reportClient,
    IAgentService agentService,
    IPlayerService playerService) : IGetService<ExportAgentWinLossRequest, Stream>
{
    public async ValueTask<Stream> GetAsync(ExportAgentWinLossRequest request,
        CancellationToken cancellationToken = default)
    {
        switch (string.IsNullOrEmpty(request.Username))
        {
            case true:
                return await reportClient.ExportMasterAgentWinLossAsync(new(
                    request.BrandId,
                    request.From,
                    request.To
                ));

            case false when !request.DownLine && request.MemberType == AccountType.Agent:
                var agent = await agentService.GetAgentAsync(x
                    => x.BrandId == request.BrandId && x.Username == request.Username);

                return await reportClient.ExportAgentWinLossAsync(new(
                    request.BrandId,
                    agent.Id,
                    request.From,
                    request.To
                ));

            case false when request.DownLine && request.MemberType == AccountType.Agent:
                var downLineAgent = await agentService.GetAgentAsync(x
                    => x.BrandId == request.BrandId && x.Username == request.Username);

                return await reportClient.ExportDownLineAgentWinLossAsync(new(
                    request.BrandId,
                    downLineAgent.Id,
                    request.From,
                    request.To
                ));

            case false when request.MemberType == AccountType.Player:
                var playerCode = await playerService.GetPlayerQueries()
                    .Where(x => x.BrandId == request.BrandId && x.Username == request.Username)
                    .Select(x => x.PlayerCode)
                    .FirstOrDefaultAsync(cancellationToken);

                return await reportClient.ExportPlayerWinLossAsync(new(
                    playerCode,
                    request.From,
                    request.To
                ));
        }

        return default;
    }
}
