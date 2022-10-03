using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public class DepositViewModel : PagedResponse<DepositFormViewModel>
{
    public decimal Sum { get; init; }
}
