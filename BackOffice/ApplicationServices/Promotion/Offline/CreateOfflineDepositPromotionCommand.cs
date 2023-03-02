using System;
using Tgs.Promotion.Clients;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
public record CreateOfflineDepositPromotionCommand(
    Guid BrandId,
    string PromotionName,
    During EventDuring,
    OfflineDepositSettings DepositSettings,
    string Modifier);
