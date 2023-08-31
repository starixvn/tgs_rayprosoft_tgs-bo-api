using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

internal class DepositFormQueryService : IGetService<DepositFormQuery, DepositForms>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public DepositFormQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask<DepositForms> GetAsync(DepositFormQuery query, CancellationToken cancellationToken)
        => _paymentServiceClient.QueryDepositFormsAsync(
            new DepositFormQueryConditions
            {
                BrandId = query.BrandId,
                ChannelName = query.ChannelName,
                CreateOnRange = query.CreateOnRange,
                PaymentId = query.PaymentId,
                PaymentMethodId = query.PaymentMethodId,
                Status = query.Status,
                ThirdPartyOrderNo = query.ThirdPartyOrderNo,
                FormId = query.FormId,
                UpdateOnRange = query.UpdateOnRange,
                UserName = query.UserName,
                OrderNo = query.OrderNo
            },
            query.Paging,
            cancellationToken);
}
