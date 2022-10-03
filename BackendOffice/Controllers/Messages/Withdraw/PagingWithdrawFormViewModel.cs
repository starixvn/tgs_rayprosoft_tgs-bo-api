using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;

public class PagingWithdrawFormViewModel : PagedResponse<WithdrawFormViewModel>
{
    public decimal Sum { get; init; }
}
