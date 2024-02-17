#nullable enable

using System;
using System.Collections.Generic;
using TGS.Services.BackOffice.Application.Models.Brand;

namespace TGS.Services.BackOffice.Domain;

public record CreateBrandInfo(
    Guid BrandId,
    IEnumerable<string> CurrencyCodes,
    IEnumerable<BrandLanguageModel> Languages,
    IEnumerable<BrandGameProviderModel> GameProviders,
    IEnumerable<string> Domains,
    IEnumerable<BrandBetLimitModel> BetLimits,
    string Modifier);