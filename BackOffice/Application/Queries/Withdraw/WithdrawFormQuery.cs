using System;
using System.Collections.Generic;
using Tgs.Payment.Clients;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

public record WithdrawFormQuery(
    Guid BrandId,
    string UserName,
    string ChannelName,
    string ThirdPartyOrderNo,
    string OrderNo,
    Guid? FormId,
    Guid? PaymentMethodId,
    IEnumerable<PaymentFlowState> Status,
    IEnumerable<Guid> BrandPaymentIds,
    UserType? UserType,
    (DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
    (DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
    PageableRequest Paging);
