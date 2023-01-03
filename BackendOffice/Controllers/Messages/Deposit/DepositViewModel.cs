using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public class DepositViewModel : PagedResponse<DepositFormViewModel>
{
    public decimal Sum { get; init; }
}
