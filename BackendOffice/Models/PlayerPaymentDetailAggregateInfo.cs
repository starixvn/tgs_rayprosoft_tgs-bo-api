using System;

namespace TGS.Apis.BackendOffice.Models;

public class PlayerPaymentDetailAggregateInfo
{
    public Guid AgentId { get; set; }

    public int DepositPlayers { get; set; }

    public int FirstDepositPlayers { get; set; }

    public int UndepositAgainPlayers { get; set; }
}
