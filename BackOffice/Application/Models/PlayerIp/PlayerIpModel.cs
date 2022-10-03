using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public record PlayerIpModel
{
    public string IpAddress { get; init; }
    public int MemberCount { get; init; }
}
