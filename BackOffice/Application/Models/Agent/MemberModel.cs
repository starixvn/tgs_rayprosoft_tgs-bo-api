using System;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class MemberModel
{
    public string Username { get; set; }
    public DateTimeOffset RegisteredOn { get; set; }
    public decimal Contribution { get; set; }
}