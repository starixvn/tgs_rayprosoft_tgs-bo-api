using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetDayNetDepositQueryService(IPaymentServiceClient paymentServiceClient) : IGetService<GetDayNetDepositQuery, DayNetDepositInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public async ValueTask<DayNetDepositInfo> GetAsync(GetDayNetDepositQuery query, CancellationToken cancellationToken = default)
    {
        var yesterday = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1);

        var todayNetDeposit = await _paymentServiceClient
            .GetBrandCurrentDayNetDepositAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        var yesterdayNetDeposit = await _paymentServiceClient
            .GetBrandReportDateNetDepositAsync(query.BrandId, DateOnly.FromDateTime(yesterday.DateTime), cancellationToken)
            .ConfigureAwait(false);

        return new DayNetDepositInfo(todayNetDeposit, yesterdayNetDeposit);
    }
}
