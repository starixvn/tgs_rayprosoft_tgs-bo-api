using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

internal class GetDepositFormDetailQueryService(IPaymentServiceClient paymentServiceClient)
    : IGetService<GetDepositFormDetailQuery, DepositFormInfo>
{
    public async ValueTask<DepositFormInfo> GetAsync(GetDepositFormDetailQuery query, CancellationToken cancellationToken = default)
    {
        var form = await paymentServiceClient.GetDepositFormAsync(
            query.FormId,
            cancellationToken).ConfigureAwait(false);

        return form.BrandId != query.BrandId ? throw new Exception("Brand Id not match") : form;
    }
}
