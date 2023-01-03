namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record AdditionalExpensesReportModel
{
    public AdditionalExpensesType AdditionalExpensesType { get; init; }

    public decimal Amount { get; init; }
}
