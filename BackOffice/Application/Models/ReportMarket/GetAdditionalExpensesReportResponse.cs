namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record GetAdditionalExpensesReportResponse
{
    public AdditionalExpensesReportModel[] AdditionalExpenses { get; init; }
}
