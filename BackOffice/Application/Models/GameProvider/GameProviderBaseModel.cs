using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GameProviderBaseModel
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public Status Status { get; set; }
    public string Description { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public string UpdatedByUsername { get; set; }
}