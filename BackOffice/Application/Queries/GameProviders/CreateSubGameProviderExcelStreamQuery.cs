using System.Collections.Generic;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public record CreateSubGameProviderExcelStreamQuery(
    IEnumerable<string> Languages,
    IEnumerable<SubGameProviderData> SubGameProviders);
