using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AgentPlayerDepositQueryService : IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics>
{
    private readonly IPaymentReportingRepository _paymentReportingRepository;

    public AgentPlayerDepositQueryService(IPaymentReportingRepository paymentReportingRepository)
    {
        _paymentReportingRepository = paymentReportingRepository ?? throw new ArgumentNullException(nameof(paymentReportingRepository));
    }

    public IAsyncEnumerable<AgentPlayerDepositStatistics> QueryAsync(
        AgentPlayerDepositQuery query,
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(8D))
            .DateTime);

        return _paymentReportingRepository.QueryAgentDailyDepositStatisticsAsync(
            query.BrandId,
            currentDate,
            cancellationToken);
    }
}
