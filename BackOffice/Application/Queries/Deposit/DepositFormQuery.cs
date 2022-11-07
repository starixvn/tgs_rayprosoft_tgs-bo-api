using System;
using System.Collections.Generic;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

public record DepositFormQuery(
    Guid BrandId,
    string UserName,
    string ChannelName,
    string ThirdPartyOrderNo,
    string OrderNo,
    Guid? FormId,
    Guid? PaymentMethodId,
    IEnumerable<PaymentFlowState> Status,
    (DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
    (DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
    Paging Paging);
