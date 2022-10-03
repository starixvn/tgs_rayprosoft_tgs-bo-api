namespace TGS.Services.BackOffice.Application.Queries.PaymentMethod;

public record CreatePaymentMethodCommand(
    string Code,
    string BankType);
