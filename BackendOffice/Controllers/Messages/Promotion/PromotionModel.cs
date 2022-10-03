using System;
using System.Collections.Generic;
using System.Text.Json;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class PromotionModel
{
    public decimal AmountUsed { get; set; }

    public bool AutoSentPlayerBonus { get; set; }

    public decimal BonusAmountLimit { get; set; }

    public Guid BrandId { get; set; }

    public string Content { get; set; }

    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string DesktopBanner { get; set; }

    public DateTimeOffset From { get; set; }

    public Guid[] GameProviderIds { get; set; }

    public GameType? GameType { get; set; }

    public Guid Id { get; set; }

    public IDictionary<string, JsonDocument> Localization { get; set; }

    public string MobileBanner { get; set; }

    public string Name { get; set; }

    public decimal Percentage { get; set; }

    public string PromotionType { get; set; }

    public SwitchStatus Status { get; set; }

    public string SubSubject { get; set; }

    public string Terms { get; set; }

    public DateTimeOffset To { get; set; }

    public decimal TotalAmountLimit { get; set; }

    public decimal TurnoverMultiple { get; set; }

    public bool Unlimited { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }

    public int[] VipLevels { get; set; }

    public int Number { get; set; }
}
