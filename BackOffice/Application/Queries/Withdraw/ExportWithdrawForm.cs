using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

public record ExportWithdrawForm(
    Guid BrandId,
    string UserName,
    string ChannelName,
    string ThirdPartyOrderNo,
    Guid? FormId,
    Guid? PaymentMethodId,
    IEnumerable<PaymentFlowState> Status,
    (DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange,
    (DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange,
    PageableRequest Paging);
