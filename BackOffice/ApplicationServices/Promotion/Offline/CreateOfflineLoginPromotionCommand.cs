using System;
using Tgs.Promotion.Clients;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
public record CreateOfflineLoginPromotionCommand(
    Guid BrandId,
    string PromotionName,
    During EventDuring,
    OfflineLoginSettings LoginSettings,
    string Modifier);
