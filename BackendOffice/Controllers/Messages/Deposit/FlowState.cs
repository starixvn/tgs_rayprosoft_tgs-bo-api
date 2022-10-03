namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public enum FlowState
{
    Unkown = 0,
    WaitingPayment,
    Review,
    Approved,
    Rejected,
    Paid,
    Invalid
}
