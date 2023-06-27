using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AdjustSummaryListItemViewModel
{
    public int Agent { get; set; }

    public decimal AgentAdjustAmount { get; set; }

    public int AgentOther { get; set; }

    public int Bonus { get; set; }

    public required string Brand { get; set; }

    public Guid BrandId { get; set; }

    public int Company { get; set; }

    public required string Currency { get; set; }

    public required string Licensee { get; set; }

    public decimal PlayerAdjustAmount { get; set; }

    public int System { get; set; }

    public int Transfer { get; set; }
}
