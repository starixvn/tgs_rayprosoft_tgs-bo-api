using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

internal class GetWithdrawFormDetailQueryService : IGetService<GetWithdrawFormDetailQuery, WithdrawFormInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public GetWithdrawFormDetailQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask<WithdrawFormInfo> GetAsync(GetWithdrawFormDetailQuery query, CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetWithdrawFormAsync(query.BrandId, query.FormId);
}
