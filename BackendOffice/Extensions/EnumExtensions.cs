using System.Collections.Generic;
using Tgs.Payment.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;
using TGS.Apis.BackendOffice.Controllers.Messages.Player;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Apis.BackendOffice.Extensions;

public static class EnumExtensions
{
    public static FlowState ToFlowState(this PaymentFlowState status)
        => status switch
        {
            PaymentFlowState.WaitPayment => FlowState.WaitingPayment,
            PaymentFlowState.WaitApprove => FlowState.Review,
            PaymentFlowState.Reject => FlowState.Rejected,
            PaymentFlowState.Approved => FlowState.Approved,
            PaymentFlowState.Paid => FlowState.Paid,
            PaymentFlowState.Init => FlowState.Invalid,
            _ => FlowState.Unkown
        };

    public static IEnumerable<PaymentFlowState> ToPaymentFlowState(this FlowState status)
        => status switch
        {
            FlowState.WaitingPayment => new[] { PaymentFlowState.WaitPayment },
            FlowState.Review => new[] { PaymentFlowState.WaitApprove },
            FlowState.Approved => new[] { PaymentFlowState.Approved },
            FlowState.Rejected => new[] { PaymentFlowState.Reject },
            FlowState.Paid => new[] { PaymentFlowState.Paid },
            FlowState.Invalid => new[] { PaymentFlowState.Init },
            _ => new[] {
                PaymentFlowState.WaitPayment,
                PaymentFlowState.WaitApprove,
                PaymentFlowState.Approved,
                PaymentFlowState.Reject,
                PaymentFlowState.Paid
            }
        };

    public static IEnumerable<PaymentFlowState> ToPaymentFlowState(this FlowState? status)
        => status?.ToPaymentFlowState() ?? new[] {
            PaymentFlowState.WaitPayment,
            PaymentFlowState.WaitApprove,
            PaymentFlowState.Approved,
            PaymentFlowState.Reject,
            PaymentFlowState.Paid
        };

    public static ReviewState ToReviewState(this ReviewResponseState reviewResponse)
        => reviewResponse switch
        {
            ReviewResponseState.Approve => ReviewState.Approve,
            _ => ReviewState.Reject
        };

    public static PlayerIdentityReviewState ToReviewState(this PlayerIdentityReviewResponseState identityReviewResponse)
        => identityReviewResponse switch
        {
            PlayerIdentityReviewResponseState.Approve => PlayerIdentityReviewState.Approve,
            _ => PlayerIdentityReviewState.Reject
        };
}
