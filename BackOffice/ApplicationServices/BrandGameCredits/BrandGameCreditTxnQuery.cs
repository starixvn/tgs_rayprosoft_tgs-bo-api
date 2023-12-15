using System;
using Tgs.BrandGameCredits.Common;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

public record BrandGameCreditTxnQuery(
     Guid BrandId,
     DateTimeOffset StartOn,
     DateTimeOffset EndOn,
     BrandGameCreditTxnTypes? Type,
     int PageNumber = 1,
     int PageSize = 20);
