using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

public record GetBrandGameProvidersReportResponse
{
    public PagedResponse<BrandGameProvidersReportModel> Datas { get; init; } = default!;

    public BrandGameProviderTotalReportModel Total { get; init; } = default!;
}
