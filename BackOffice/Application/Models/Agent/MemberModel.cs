using System;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class MemberModel
{
    public decimal Contribution { get; set; }

    public DateTimeOffset RegisteredOn { get; set; }

    public string Username { get; set; }
}
