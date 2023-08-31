using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

internal class GetDepositFormDetailQueryService : IGetService<GetDepositFormDetailQuery, DepositFormInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public GetDepositFormDetailQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask<DepositFormInfo> GetAsync(GetDepositFormDetailQuery query, CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetDepositFormAsync(query.BrandId, query.FormId, cancellationToken);
}
