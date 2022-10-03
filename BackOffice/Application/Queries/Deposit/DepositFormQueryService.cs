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
                PaymentMethodId = query.PaymentMethodId,
                Status = query.Status,
                ThirdPartyOrderNo = query.ThirdPartyOrderNo,
                FormId = query.FormId,
                UpdateOnRange = query.UpdateOnRange,
                UserName = query.UserName,
                OrderNo = query.OrderNo
            },
            new Tgs.Payment.Clients.Paging
            {
                IsDesc = query.Paging.IsDesc,
                Page = query.Paging.Page,
                Rows = query.Paging.Rows,
                ShowAll = query.Paging.ShowAll,
                SortBy = query.Paging.SortBy
            });
}
