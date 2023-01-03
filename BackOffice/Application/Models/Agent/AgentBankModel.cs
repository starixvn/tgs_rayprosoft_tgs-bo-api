using System;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentBankModel
{
    public string BankAccount { get; set; }

    public string BankName { get; set; }

    public Guid Id { get; set; }

    public string RealName { get; set; }
}
