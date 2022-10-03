using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

internal class WithdrawFormQueryService : IGetService<WithdrawFormQuery, WithdrawForms>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public WithdrawFormQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask<WithdrawForms> GetAsync(WithdrawFormQuery query, CancellationToken cancellationToken)
        => _paymentServiceClient.QueryWithdrawFormsAsync(new WithdrawFormQueryConditions
        {
            BrandId = query.BrandId,
            ChannelName = query.ChannelName,
            CreateOnRange = query.CreateOnRange,
            PaymentMethodId = query.PaymentMethodId,
            Status = query.Status,
            BrandPaymentIds = query.BrandPaymentIds,
            ThirdPartyOrderNo = query.ThirdPartyOrderNo,
            OrderNo = query.OrderNo,
            FormId = query.FormId,
            UpdateOnRange = query.UpdateOnRange,
            UserName = query.UserName,
            UserType = query.UserType == null ? null : (Tgs.Payment.Clients.UserType)query.UserType,
        }, new Paging
        {
            IsDesc = query.Paging.IsDesc,
            Page = query.Paging.Page,
            Rows = query.Paging.Rows,
            ShowAll = query.Paging.ShowAll,
            SortBy = query.Paging.SortBy
        }, cancellationToken);
}
