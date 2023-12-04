using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetDaySuccessRateQueryService(IPaymentServiceClient paymentServiceClient) : IGetService<GetDaySuccessRateQuery, DaySuccessRateInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public async ValueTask<DaySuccessRateInfo> GetAsync(GetDaySuccessRateQuery query, CancellationToken cancellationToken = default)
    {
        var yesterday = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1);

        var todayData = await _paymentServiceClient
            .GetCurrentDayDepositSuccessRateAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        var yesterdayData = await _paymentServiceClient
            .GetBrandSuccessRateByReportDateAsync(query.BrandId, DateOnly.FromDateTime(yesterday.DateTime), cancellationToken)
            .ConfigureAwait(false);

        return new DaySuccessRateInfo(todayData, yesterdayData);
    }
}
